using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CsvUtility
{
    public sealed class CsvReader : ReaderBase
    {
        private CsvReader.ColumnBuffer columnBuffer;
        private CsvReader.DataBuffer dataBuffer;
        private bool detectBom;
        private Encoding encoding;
        private string fileName;
        private bool hasMoreData;
        private bool hasReadNextLine;
        private CsvReader.HeadersHolder headersHolder;
        private bool initialized;
        private TextReader inputStream;
        private bool[] isQualified;
        private char lastLetter;
        private CsvReader.RawRecordBuffer rawBuffer;
        private string rawRecord;
        private bool startedColumn;
        private bool startedWithQualifier;
        private CsvReader.UserSettings userSettings;

        private CsvReader()
        {
            this.inputStream = (TextReader)null;
            this.fileName = (string)null;
            this.userSettings = new CsvReader.UserSettings();
            this.encoding = (Encoding)null;
            this.detectBom = false;
            this.dataBuffer = CsvReader.DataBuffer.Create();
            this.columnBuffer = CsvReader.ColumnBuffer.Create();
            this.rawBuffer = CsvReader.RawRecordBuffer.Create();
            this.isQualified = (bool[])null;
            this.rawRecord = "";
            this.headersHolder = CsvReader.HeadersHolder.Create();
            this.startedColumn = false;
            this.startedWithQualifier = false;
            this.hasMoreData = true;
            this.lastLetter = char.MinValue;
            this.hasReadNextLine = false;
            this.initialized = false;
            this.isQualified = new bool[this.values.Length];
            this.userSettings.SettingChanged += new CsvReader.UserSettings.SettingChangedEventHandler(this.SettingChangedHandler);
        }

        public CsvReader(TextReader inputStream)
          : this(inputStream, ',')
        {
        }

        public CsvReader(string fileName)
          : this(fileName, ',')
        {
        }

        public CsvReader(Stream inputStream, Encoding encoding)
          : this((TextReader)new StreamReader(inputStream, encoding, false))
        {
        }

        public CsvReader(TextReader inputStream, char delimiter)
          : this()
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream), "Input stream can not be null.");
            this.inputStream = inputStream;
            this.userSettings.delimiter = delimiter;
            this.initialized = true;
        }

        public CsvReader(string fileName, char delimiter)
          : this(fileName, delimiter, Encoding.Default)
        {
            this.detectBom = true;
        }

        public CsvReader(Stream inputStream, char delimiter, Encoding encoding)
          : this((TextReader)new StreamReader(inputStream, encoding, false), delimiter)
        {
        }

        public CsvReader(string fileName, char delimiter, Encoding encoding)
          : this()
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName), "File name can not be null.");
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding), "Encoding can not be null.");
            if (!File.Exists(fileName))
                throw new FileNotFoundException("File " + fileName + " does not exist.");
            this.fileName = fileName;
            this.userSettings.delimiter = delimiter;
            this.encoding = encoding;
        }

        private void AppendLetter(char letter)
        {
            if (this.columnBuffer.Position == this.columnBuffer.Buffer.Length)
            {
                char[] chArray = new char[this.columnBuffer.Buffer.Length * 2];
                Array.Copy((Array)this.columnBuffer.Buffer, 0, (Array)chArray, 0, this.columnBuffer.Position);
                this.columnBuffer.Buffer = chArray;
            }
            this.columnBuffer.Buffer[this.columnBuffer.Position++] = letter;
            this.dataBuffer.ColumnStart = this.dataBuffer.Position + 1;
        }

        private void CheckDataLength()
        {
            if (!this.initialized)
            {
                if (this.fileName != null)
                    this.inputStream = (TextReader)new StreamReader((Stream)new FileStream(this.fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, false), this.encoding, this.detectBom);
                this.encoding = (Encoding)null;
                this.initialized = true;
            }
            this.UpdateCurrentValue();
            if (this.userSettings.captureRawRecord && this.dataBuffer.Count > 0)
            {
                if (this.rawBuffer.Buffer.Length - this.rawBuffer.Position < this.dataBuffer.Count - this.dataBuffer.LineStart)
                {
                    char[] chArray = new char[this.rawBuffer.Buffer.Length + Math.Max(this.dataBuffer.Count - this.dataBuffer.LineStart, this.rawBuffer.Buffer.Length)];
                    Array.Copy((Array)this.rawBuffer.Buffer, 0, (Array)chArray, 0, this.rawBuffer.Position);
                    this.rawBuffer.Buffer = chArray;
                }
                Array.Copy((Array)this.dataBuffer.Buffer, this.dataBuffer.LineStart, (Array)this.rawBuffer.Buffer, this.rawBuffer.Position, this.dataBuffer.Count - this.dataBuffer.LineStart);
                this.rawBuffer.Position += this.dataBuffer.Count - this.dataBuffer.LineStart;
            }
            try
            {
                this.dataBuffer.Count = this.inputStream.Read(this.dataBuffer.Buffer, 0, this.dataBuffer.Buffer.Length);
            }
            catch
            {
                this.Close();
                throw;
            }
            if (this.dataBuffer.Count == 0)
                this.hasMoreData = false;
            this.dataBuffer.Position = 0;
            this.dataBuffer.LineStart = 0;
            this.dataBuffer.ColumnStart = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
                return;
            if (disposing)
            {
                this.encoding = (Encoding)null;
                this.headersHolder.Headers = (string[])null;
                this.headersHolder.IndexByName = (Hashtable)null;
                this.dataBuffer.Buffer = (char[])null;
                this.columnBuffer.Buffer = (char[])null;
                this.rawBuffer.Buffer = (char[])null;
            }
            if (this.initialized)
                this.inputStream.Dispose();
            this.inputStream = (TextReader)null;
            this.disposed = true;
        }

        private void EndColumn()
        {
            string str = "";
            if (this.startedColumn)
            {
                if (this.columnBuffer.Position == 0)
                {
                    if (this.dataBuffer.ColumnStart < this.dataBuffer.Position)
                    {
                        int index = this.dataBuffer.Position - 1;
                        if (this.userSettings.trimWhitespace && !this.startedWithQualifier)
                        {
                            while (index >= this.dataBuffer.ColumnStart && ((int)this.dataBuffer.Buffer[index] == 32 || (int)this.dataBuffer.Buffer[index] == 9))
                                --index;
                        }
                        str = new string(this.dataBuffer.Buffer, this.dataBuffer.ColumnStart, index - this.dataBuffer.ColumnStart + 1);
                    }
                }
                else
                {
                    this.UpdateCurrentValue();
                    int index = this.columnBuffer.Position - 1;
                    if (this.userSettings.trimWhitespace && !this.startedWithQualifier)
                    {
                        while (index >= 0 && ((int)this.columnBuffer.Buffer[index] == 32 || (int)this.columnBuffer.Buffer[index] == 9))
                            --index;
                    }
                    str = new string(this.columnBuffer.Buffer, 0, index + 1);
                }
            }
            this.columnBuffer.Position = 0;
            this.startedColumn = false;
            if (this.columnsCount >= 100000 && this.userSettings.safetySwitch)
            {
                this.Close();
                throw new IOException("Maximum column count of 100,000 exceeded in record " + this.currentRecord.ToString("###,##0") + ". Set the SafetySwitch property to false if you're expecting more than 100,000 columns per record to avoid this error.");
            }
            if (this.columnsCount == this.values.Length)
            {
                int length = this.values.Length * 2;
                string[] strArray = new string[length];
                Array.Copy((Array)this.values, 0, (Array)strArray, 0, this.values.Length);
                this.values = strArray;
                bool[] flagArray = new bool[length];
                Array.Copy((Array)this.isQualified, 0, (Array)flagArray, 0, this.isQualified.Length);
                this.isQualified = flagArray;
            }
            this.values[this.columnsCount] = str;
            this.isQualified[this.columnsCount] = this.startedWithQualifier;
            ++this.columnsCount;
        }

        private void EndRecord()
        {
            this.hasReadNextLine = true;
            ++this.currentRecord;
        }

        ~CsvReader()
        {
            this.Dispose(false);
        }

        public string GetHeader(int columnIndex)
        {
            this.CheckDisposed();
            if (columnIndex > -1 && columnIndex < this.headersHolder.Length)
                return this.headersHolder.Headers[columnIndex];
            return "";
        }

        public int GetIndex(string headerName)
        {
            this.CheckDisposed();
            if (this.headersHolder.IndexByName.ContainsKey((object)headerName))
                return (int)this.headersHolder.IndexByName[(object)headerName];
            return -1;
        }

        private static char HexToDec(char hex)
        {
            if ((int)hex >= 97)
                return (char)((int)hex - 97 + 10);
            if ((int)hex >= 65)
                return (char)((int)hex - 65 + 10);
            return (char)((uint)hex - 48U);
        }

        public bool IsQualified(int columnIndex)
        {
            this.CheckDisposed();
            return columnIndex < this.columnsCount && columnIndex > -1 && this.isQualified[columnIndex];
        }

        public static CsvReader Parse(string data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");
            return new CsvReader((TextReader)new StringReader(data));
        }

        public bool ReadHeaders()
        {
            bool flag = this.ReadRecord();
            this.headersHolder.Length = this.columnsCount;
            this.headersHolder.Headers = new string[this.columnsCount];
            this.headersHolder.IndexByName.Clear();
            for (int index = 0; index < this.headersHolder.Length; ++index)
            {
                string str = this[index];
                this.headersHolder.Headers[index] = str;
                this.headersHolder.IndexByName[(object)str] = (object)index;
            }
            if (flag)
                --this.currentRecord;
            this.columnsCount = 0;
            return flag;
        }

        public override bool ReadRecord()
        {
            this.CheckDisposed();
            this.columnsCount = 0;
            this.rawBuffer.Position = 0;
            this.dataBuffer.LineStart = this.dataBuffer.Position;
            this.hasReadNextLine = false;
            if (this.hasMoreData)
            {
                do
                {
                    if (this.dataBuffer.Position == this.dataBuffer.Count)
                    {
                        this.CheckDataLength();
                    }
                    else
                    {
                        this.startedWithQualifier = false;
                        char hex1 = this.dataBuffer.Buffer[this.dataBuffer.Position];
                        if (!this.userSettings.useTextQualifier || (int)hex1 != (int)this.userSettings.textQualifier)
                        {
                            if ((int)hex1 == (int)this.userSettings.delimiter)
                            {
                                this.lastLetter = hex1;
                                this.EndColumn();
                            }
                            else if (this.userSettings.useCustomRecordDelimiter && (int)hex1 == (int)this.userSettings.recordDelimiter)
                            {
                                if (this.startedColumn || this.columnsCount > 0 || !this.userSettings.skipEmptyRecords)
                                {
                                    this.EndColumn();
                                    this.EndRecord();
                                }
                                else
                                    this.dataBuffer.LineStart = this.dataBuffer.Position + 1;
                                this.lastLetter = hex1;
                            }
                            else if (!this.userSettings.useCustomRecordDelimiter && ((int)hex1 == 13 || (int)hex1 == 10))
                            {
                                if (this.startedColumn || this.columnsCount > 0 || !this.userSettings.skipEmptyRecords && ((int)hex1 == 13 || (int)this.lastLetter != 13))
                                {
                                    this.EndColumn();
                                    this.EndRecord();
                                }
                                else
                                    this.dataBuffer.LineStart = this.dataBuffer.Position + 1;
                                this.lastLetter = hex1;
                            }
                            else if (this.userSettings.useComments && this.columnsCount == 0 && (int)hex1 == (int)this.userSettings.comment)
                            {
                                this.lastLetter = hex1;
                                this.SkipLine();
                            }
                            else if (this.userSettings.trimWhitespace && ((int)hex1 == 32 || (int)hex1 == 9))
                            {
                                this.startedColumn = true;
                                this.dataBuffer.ColumnStart = this.dataBuffer.Position + 1;
                            }
                            else
                            {
                                this.startedColumn = true;
                                this.dataBuffer.ColumnStart = this.dataBuffer.Position;
                                bool flag1 = false;
                                bool flag2 = false;
                                CsvReader.ComplexEscape complexEscape = CsvReader.ComplexEscape.Unicode;
                                int num = 0;
                                char letter = char.MinValue;
                                bool flag3 = true;
                                do
                                {
                                    if (!flag3 && this.dataBuffer.Position == this.dataBuffer.Count)
                                    {
                                        this.CheckDataLength();
                                    }
                                    else
                                    {
                                        if (!flag3)
                                            hex1 = this.dataBuffer.Buffer[this.dataBuffer.Position];
                                        if (!this.userSettings.useTextQualifier && this.userSettings.escapeMode == EscapeMode.Backslash && (int)hex1 == 92)
                                        {
                                            if (flag1)
                                            {
                                                flag1 = false;
                                            }
                                            else
                                            {
                                                this.UpdateCurrentValue();
                                                flag1 = true;
                                            }
                                        }
                                        else if (flag2)
                                        {
                                            ++num;
                                            switch (complexEscape)
                                            {
                                                case CsvReader.ComplexEscape.Unicode:
                                                    letter = (char)((uint)(char)((uint)letter * 16U) + (uint)CsvReader.HexToDec(hex1));
                                                    if (num == 4)
                                                    {
                                                        flag2 = false;
                                                        break;
                                                    }
                                                    break;
                                                case CsvReader.ComplexEscape.Octal:
                                                    letter = (char)((uint)(char)((uint)letter * 8U) + (uint)(ushort)((uint)hex1 - 48U));
                                                    if (num == 3)
                                                    {
                                                        flag2 = false;
                                                        break;
                                                    }
                                                    break;
                                                case CsvReader.ComplexEscape.Decimal:
                                                    letter = (char)((uint)(char)((uint)letter * 10U) + (uint)(ushort)((uint)hex1 - 48U));
                                                    if (num == 3)
                                                    {
                                                        flag2 = false;
                                                        break;
                                                    }
                                                    break;
                                                case CsvReader.ComplexEscape.Hex:
                                                    letter = (char)((uint)(char)((uint)letter * 16U) + (uint)CsvReader.HexToDec(hex1));
                                                    if (num == 2)
                                                    {
                                                        flag2 = false;
                                                        break;
                                                    }
                                                    break;
                                            }
                                            if (!flag2)
                                                this.AppendLetter(letter);
                                            else
                                                this.dataBuffer.ColumnStart = this.dataBuffer.Position + 1;
                                        }
                                        else if (this.userSettings.escapeMode != EscapeMode.Backslash || !flag1)
                                        {
                                            if ((int)hex1 == (int)this.userSettings.delimiter)
                                                this.EndColumn();
                                            else if (!this.userSettings.useCustomRecordDelimiter && ((int)hex1 == 13 || (int)hex1 == 10) || this.userSettings.useCustomRecordDelimiter && (int)hex1 == (int)this.userSettings.recordDelimiter)
                                            {
                                                this.EndColumn();
                                                this.EndRecord();
                                            }
                                        }
                                        else
                                        {
                                            switch (hex1)
                                            {
                                                case '0':
                                                case '1':
                                                case '2':
                                                case '3':
                                                case '4':
                                                case '5':
                                                case '6':
                                                case '7':
                                                    complexEscape = CsvReader.ComplexEscape.Octal;
                                                    flag2 = true;
                                                    num = 1;
                                                    letter = (char)((uint)hex1 - 48U);
                                                    this.dataBuffer.ColumnStart = this.dataBuffer.Position + 1;
                                                    break;
                                                case 'D':
                                                case 'O':
                                                case 'U':
                                                case 'X':
                                                case 'd':
                                                case 'o':
                                                case 'u':
                                                case 'x':
                                                    switch (hex1)
                                                    {
                                                        case 'D':
                                                        case 'd':
                                                            complexEscape = CsvReader.ComplexEscape.Decimal;
                                                            break;
                                                        case 'O':
                                                        case 'o':
                                                            complexEscape = CsvReader.ComplexEscape.Octal;
                                                            break;
                                                        case 'U':
                                                        case 'u':
                                                            complexEscape = CsvReader.ComplexEscape.Unicode;
                                                            break;
                                                        case 'X':
                                                        case 'x':
                                                            complexEscape = CsvReader.ComplexEscape.Hex;
                                                            break;
                                                    }
                                                    flag2 = true;
                                                    num = 0;
                                                    letter = char.MinValue;
                                                    this.dataBuffer.ColumnStart = this.dataBuffer.Position + 1;
                                                    break;
                                                case 'a':
                                                    this.AppendLetter('\a');
                                                    break;
                                                case 'b':
                                                    this.AppendLetter('\b');
                                                    break;
                                                case 'e':
                                                    this.AppendLetter('\x001B');
                                                    break;
                                                case 'f':
                                                    this.AppendLetter('\f');
                                                    break;
                                                case 'n':
                                                    this.AppendLetter('\n');
                                                    break;
                                                case 'r':
                                                    this.AppendLetter('\r');
                                                    break;
                                                case 't':
                                                    this.AppendLetter('\t');
                                                    break;
                                                case 'v':
                                                    this.AppendLetter('\v');
                                                    break;
                                            }
                                            flag1 = false;
                                        }
                                        this.lastLetter = hex1;
                                        flag3 = false;
                                        if (this.startedColumn)
                                        {
                                            ++this.dataBuffer.Position;
                                            if (this.userSettings.safetySwitch && this.dataBuffer.Position - this.dataBuffer.ColumnStart + this.columnBuffer.Position > 100000)
                                            {
                                                this.Close();
                                                throw new IOException("Maximum column length of 100,000 exceeded in column " + this.columnsCount.ToString("###,##0") + " in record " + this.currentRecord.ToString("###,##0") + ". Set the SafetySwitch property to false if you're expecting column lengths greater than 100,000 characters to avoid this error.");
                                            }
                                        }
                                    }
                                }
                                while (this.hasMoreData && this.startedColumn);
                            }
                        }
                        else
                        {
                            this.lastLetter = hex1;
                            this.startedColumn = true;
                            this.dataBuffer.ColumnStart = this.dataBuffer.Position + 1;
                            this.startedWithQualifier = true;
                            bool flag1 = false;
                            char ch = this.userSettings.textQualifier;
                            if (this.userSettings.escapeMode == EscapeMode.Backslash)
                                ch = '\\';
                            bool flag2 = false;
                            bool flag3 = false;
                            bool flag4 = false;
                            CsvReader.ComplexEscape complexEscape = CsvReader.ComplexEscape.Unicode;
                            int num = 0;
                            char letter = char.MinValue;
                            ++this.dataBuffer.Position;
                            do
                            {
                                if (this.dataBuffer.Position == this.dataBuffer.Count)
                                {
                                    this.CheckDataLength();
                                }
                                else
                                {
                                    char hex2 = this.dataBuffer.Buffer[this.dataBuffer.Position];
                                    if (flag2)
                                    {
                                        this.dataBuffer.ColumnStart = this.dataBuffer.Position + 1;
                                        if ((int)hex2 == (int)this.userSettings.delimiter)
                                            this.EndColumn();
                                        else if (!this.userSettings.useCustomRecordDelimiter && ((int)hex2 == 13 || (int)hex2 == 10) || this.userSettings.useCustomRecordDelimiter && (int)hex2 == (int)this.userSettings.recordDelimiter)
                                        {
                                            this.EndColumn();
                                            this.EndRecord();
                                        }
                                    }
                                    else if (flag4)
                                    {
                                        ++num;
                                        switch (complexEscape)
                                        {
                                            case CsvReader.ComplexEscape.Unicode:
                                                letter = (char)((uint)(char)((uint)letter * 16U) + (uint)CsvReader.HexToDec(hex2));
                                                if (num == 4)
                                                {
                                                    flag4 = false;
                                                    break;
                                                }
                                                break;
                                            case CsvReader.ComplexEscape.Octal:
                                                letter = (char)((uint)(char)((uint)letter * 8U) + (uint)(ushort)((uint)hex2 - 48U));
                                                if (num == 3)
                                                {
                                                    flag4 = false;
                                                    break;
                                                }
                                                break;
                                            case CsvReader.ComplexEscape.Decimal:
                                                letter = (char)((uint)(char)((uint)letter * 10U) + (uint)(ushort)((uint)hex2 - 48U));
                                                if (num == 3)
                                                {
                                                    flag4 = false;
                                                    break;
                                                }
                                                break;
                                            case CsvReader.ComplexEscape.Hex:
                                                letter = (char)((uint)(char)((uint)letter * 16U) + (uint)CsvReader.HexToDec(hex2));
                                                if (num == 2)
                                                {
                                                    flag4 = false;
                                                    break;
                                                }
                                                break;
                                        }
                                        if (!flag4)
                                            this.AppendLetter(letter);
                                        else
                                            this.dataBuffer.ColumnStart = this.dataBuffer.Position + 1;
                                    }
                                    else if ((int)hex2 == (int)this.userSettings.textQualifier)
                                    {
                                        if (flag3)
                                        {
                                            flag3 = false;
                                            flag1 = false;
                                        }
                                        else
                                        {
                                            this.UpdateCurrentValue();
                                            if (this.userSettings.escapeMode == EscapeMode.Doubled)
                                                flag3 = true;
                                            flag1 = true;
                                        }
                                    }
                                    else if (this.userSettings.escapeMode != EscapeMode.Backslash || !flag3)
                                    {
                                        if ((int)hex2 == (int)ch)
                                        {
                                            this.UpdateCurrentValue();
                                            flag3 = true;
                                        }
                                        else if (flag1)
                                        {
                                            if ((int)hex2 == (int)this.userSettings.delimiter)
                                                this.EndColumn();
                                            else if (!this.userSettings.useCustomRecordDelimiter && ((int)hex2 == 13 || (int)hex2 == 10) || this.userSettings.useCustomRecordDelimiter && (int)hex2 == (int)this.userSettings.recordDelimiter)
                                            {
                                                this.EndColumn();
                                                this.EndRecord();
                                            }
                                            else
                                            {
                                                this.dataBuffer.ColumnStart = this.dataBuffer.Position + 1;
                                                flag2 = true;
                                            }
                                            flag1 = false;
                                        }
                                    }
                                    else
                                    {
                                        switch (hex2)
                                        {
                                            case '0':
                                            case '1':
                                            case '2':
                                            case '3':
                                            case '4':
                                            case '5':
                                            case '6':
                                            case '7':
                                                complexEscape = CsvReader.ComplexEscape.Octal;
                                                flag4 = true;
                                                num = 1;
                                                letter = (char)((uint)hex2 - 48U);
                                                this.dataBuffer.ColumnStart = this.dataBuffer.Position + 1;
                                                break;
                                            case 'D':
                                            case 'O':
                                            case 'U':
                                            case 'X':
                                            case 'd':
                                            case 'o':
                                            case 'u':
                                            case 'x':
                                                switch (hex2)
                                                {
                                                    case 'D':
                                                    case 'd':
                                                        complexEscape = CsvReader.ComplexEscape.Decimal;
                                                        break;
                                                    case 'O':
                                                    case 'o':
                                                        complexEscape = CsvReader.ComplexEscape.Octal;
                                                        break;
                                                    case 'U':
                                                    case 'u':
                                                        complexEscape = CsvReader.ComplexEscape.Unicode;
                                                        break;
                                                    case 'X':
                                                    case 'x':
                                                        complexEscape = CsvReader.ComplexEscape.Hex;
                                                        break;
                                                }
                                                flag4 = true;
                                                num = 0;
                                                letter = char.MinValue;
                                                this.dataBuffer.ColumnStart = this.dataBuffer.Position + 1;
                                                break;
                                            case 'a':
                                                this.AppendLetter('\a');
                                                break;
                                            case 'b':
                                                this.AppendLetter('\b');
                                                break;
                                            case 'e':
                                                this.AppendLetter('\x001B');
                                                break;
                                            case 'f':
                                                this.AppendLetter('\f');
                                                break;
                                            case 'n':
                                                this.AppendLetter('\n');
                                                break;
                                            case 'r':
                                                this.AppendLetter('\r');
                                                break;
                                            case 't':
                                                this.AppendLetter('\t');
                                                break;
                                            case 'v':
                                                this.AppendLetter('\v');
                                                break;
                                        }
                                        flag3 = false;
                                    }
                                    this.lastLetter = hex2;
                                    if (this.startedColumn)
                                    {
                                        ++this.dataBuffer.Position;
                                        if (this.userSettings.safetySwitch && this.dataBuffer.Position - this.dataBuffer.ColumnStart + this.columnBuffer.Position > 100000)
                                        {
                                            this.Close();
                                            throw new IOException("Maximum column length of 100,000 exceeded in column " + this.columnsCount.ToString("###,##0") + " in record " + this.currentRecord.ToString("###,##0") + ". Set the SafetySwitch property to false if you're expecting column lengths greater than 100,000 characters to avoid this error.");
                                        }
                                    }
                                }
                            }
                            while (this.hasMoreData && this.startedColumn);
                        }
                        if (this.hasMoreData)
                            ++this.dataBuffer.Position;
                    }
                }
                while (this.hasMoreData && !this.hasReadNextLine);
                if (this.startedColumn || (int)this.lastLetter == (int)this.userSettings.delimiter)
                {
                    this.EndColumn();
                    this.EndRecord();
                }
            }
            this.rawRecord = !this.userSettings.captureRawRecord ? "" : (!this.hasMoreData ? new string(this.rawBuffer.Buffer, 0, this.rawBuffer.Position) : (this.rawBuffer.Position != 0 ? new string(this.rawBuffer.Buffer, 0, this.rawBuffer.Position) + new string(this.dataBuffer.Buffer, this.dataBuffer.LineStart, this.dataBuffer.Position - this.dataBuffer.LineStart - 1) : new string(this.dataBuffer.Buffer, this.dataBuffer.LineStart, this.dataBuffer.Position - this.dataBuffer.LineStart - 1)));
            return this.hasReadNextLine;
        }

        public DataTable ReadToEnd()
        {
            return this.ReadToEnd(true);
        }

        public DataTable ReadToEnd(bool readHeaders)
        {
            return this.ReadToEnd(readHeaders, 0UL);
        }

        public DataTable ReadToEnd(bool readHeaders, ulong maxRecords)
        {
            DataTable dataTable = new DataTable();
            dataTable.BeginLoadData();
            if (readHeaders)
            {
                this.ReadHeaders();
                bool flag = true;
                for (int index1 = 0; index1 < this.headersHolder.Length; ++index1)
                {
                    if (flag)
                    {
                        string str = this.headersHolder.Headers[index1];
                        if (str.Length == 0)
                            str = "Column" + (object)(index1 + 1);
                        if (dataTable.Columns.Contains(str))
                        {
                            for (int index2 = index1 - 1; index2 >= 0; --index2)
                                dataTable.Columns[index2].ColumnName = "Column" + (object)(index2 + 1);
                            flag = false;
                        }
                        else
                            dataTable.Columns.Add(str);
                    }
                    if (!flag)
                        dataTable.Columns.Add("Column" + (object)(index1 + 1));
                }
            }
            int num = this.headersHolder.Length;
            bool flag1 = maxRecords > 0UL;
            DataRowCollection rows = dataTable.Rows;
            while ((!flag1 || this.currentRecord < maxRecords) && this.ReadRecord())
            {
                if (this.columnsCount > num)
                {
                    for (int index1 = num; index1 < this.columnsCount; ++index1)
                    {
                        dataTable.Columns.Add("Column" + (object)(index1 + 1));
                        for (int index2 = 0; index2 < (int)this.currentRecord - 1; ++index2)
                            rows[index2][index1] = (object)"";
                    }
                    num = this.columnsCount;
                }
                rows.Add((object[])this.Values);
                for (int columnsCount = this.columnsCount; columnsCount < num; ++columnsCount)
                    rows[rows.Count - 1][columnsCount] = (object)"";
            }
            dataTable.EndLoadData();
            return dataTable;
        }

        private void SettingChangedHandler(CsvReader.UserSettings.Setting setting, object previousValue, object newValue)
        {
            if (setting != CsvReader.UserSettings.Setting.CaseSensitive)
                return;
            this.headersHolder.IndexByName = !(bool)newValue ? new Hashtable((IDictionary)this.headersHolder.IndexByName, (IHashCodeProvider)CaseInsensitiveHashCodeProvider.Default, (IComparer)CaseInsensitiveComparer.Default) : new Hashtable((IDictionary)this.headersHolder.IndexByName);
        }

        public bool SkipLine()
        {
            this.CheckDisposed();
            this.columnsCount = 0;
            bool flag1 = false;
            if (this.hasMoreData)
            {
                bool flag2 = false;
                do
                {
                    if (this.dataBuffer.Position == this.dataBuffer.Count)
                    {
                        this.CheckDataLength();
                    }
                    else
                    {
                        flag1 = true;
                        char ch = this.dataBuffer.Buffer[this.dataBuffer.Position];
                        switch (ch)
                        {
                            case '\n':
                            case '\r':
                                flag2 = true;
                                break;
                        }
                        this.lastLetter = ch;
                        if (!flag2)
                            ++this.dataBuffer.Position;
                    }
                }
                while (this.hasMoreData && !flag2);
                this.columnBuffer.Position = 0;
                this.dataBuffer.LineStart = this.dataBuffer.Position + 1;
            }
            this.rawBuffer.Position = 0;
            this.rawRecord = "";
            return flag1;
        }

        public bool SkipRecord()
        {
            this.CheckDisposed();
            bool flag = false;
            if (this.hasMoreData)
            {
                flag = this.ReadRecord();
                if (flag)
                    --this.currentRecord;
            }
            return flag;
        }

        private void UpdateCurrentValue()
        {
            if (this.startedColumn && this.dataBuffer.ColumnStart < this.dataBuffer.Position)
            {
                if (this.columnBuffer.Buffer.Length - this.columnBuffer.Position < this.dataBuffer.Position - this.dataBuffer.ColumnStart)
                {
                    char[] chArray = new char[this.columnBuffer.Buffer.Length + Math.Max(this.dataBuffer.Position - this.dataBuffer.ColumnStart, this.columnBuffer.Buffer.Length)];
                    Array.Copy((Array)this.columnBuffer.Buffer, 0, (Array)chArray, 0, this.columnBuffer.Position);
                    this.columnBuffer.Buffer = chArray;
                }
                Array.Copy((Array)this.dataBuffer.Buffer, this.dataBuffer.ColumnStart, (Array)this.columnBuffer.Buffer, this.columnBuffer.Position, this.dataBuffer.Position - this.dataBuffer.ColumnStart);
                this.columnBuffer.Position += this.dataBuffer.Position - this.dataBuffer.ColumnStart;
            }
            this.dataBuffer.ColumnStart = this.dataBuffer.Position + 1;
        }

        public int ColumnCount
        {
            get
            {
                return this.columnsCount;
            }
        }

        public int HeaderCount
        {
            get
            {
                return this.headersHolder.Length;
            }
        }

        public string[] Headers
        {
            get
            {
                this.CheckDisposed();
                if (this.headersHolder.Headers == null)
                    return (string[])null;
                string[] strArray = new string[this.headersHolder.Length];
                Array.Copy((Array)this.headersHolder.Headers, (Array)strArray, this.headersHolder.Length);
                return strArray;
            }
            set
            {
                this.headersHolder.Headers = value;
                this.headersHolder.IndexByName.Clear();
                this.headersHolder.Length = value == null ? 0 : value.Length;
                for (int index = 0; index < this.headersHolder.Length; ++index)
                    this.headersHolder.IndexByName[(object)value[index]] = (object)index;
            }
        }

        public string this[string headerName]
        {
            get
            {
                this.CheckDisposed();
                return this[this.GetIndex(headerName)];
            }
        }

        public string RawRecord
        {
            get
            {
                return this.rawRecord;
            }
        }

        public CsvReader.UserSettings Settings
        {
            get
            {
                return this.userSettings;
            }
        }

        private struct ColumnBuffer
        {
            public char[] Buffer;
            public int Position;

            public static CsvReader.ColumnBuffer Create()
            {
                return new CsvReader.ColumnBuffer()
                {
                    Buffer = new char[CsvReader.StaticSettings.InitialColumnBufferSize],
                    Position = 0
                };
            }
        }

        private enum ComplexEscape
        {
            Unicode,
            Octal,
            Decimal,
            Hex,
        }

        private struct DataBuffer
        {
            public char[] Buffer;
            public int Position;
            public int Count;
            public int ColumnStart;
            public int LineStart;

            public static CsvReader.DataBuffer Create()
            {
                return new CsvReader.DataBuffer()
                {
                    Buffer = new char[CsvReader.StaticSettings.MaxBufferSize],
                    Position = 0,
                    Count = 0,
                    ColumnStart = 0,
                    LineStart = 0
                };
            }
        }

        private struct HeadersHolder
        {
            public string[] Headers;
            public int Length;
            public Hashtable IndexByName;

            public static CsvReader.HeadersHolder Create()
            {
                return new CsvReader.HeadersHolder()
                {
                    Headers = (string[])null,
                    Length = 0,
                    IndexByName = new Hashtable()
                };
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct Letters
        {
            public const char Lf = '\n';
            public const char Cr = '\r';
            public const char Quote = '"';
            public const char Comma = ',';
            public const char Space = ' ';
            public const char Tab = '\t';
            public const char Pound = '#';
            public const char Backslash = '\\';
            public const char Null = '\0';
            public const char Backspace = '\b';
            public const char FormFeed = '\f';
            public const char Escape = '\x001B';
            public const char VerticalTab = '\v';
            public const char Alert = '\a';
        }

        private struct RawRecordBuffer
        {
            public char[] Buffer;
            public int Position;

            public static CsvReader.RawRecordBuffer Create()
            {
                return new CsvReader.RawRecordBuffer()
                {
                    Buffer = new char[CsvReader.StaticSettings.InitialColumnBufferSize * CsvReader.StaticSettings.InitialColumnCount],
                    Position = 0
                };
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct StaticSettings
        {
            public static int MaxBufferSize = 1024;
            public static int InitialColumnCount = 10;
            public static int InitialColumnBufferSize = 50;
            public const int MaxFileBufferSize = 4096;
        }

        public class UserSettings
        {
            internal bool captureRawRecord = true;
            internal bool caseSensitive = true;
            internal char comment = '#';
            internal char delimiter = ',';
            internal EscapeMode escapeMode = EscapeMode.Doubled;
            internal char recordDelimiter = char.MinValue;
            internal bool safetySwitch = true;
            internal bool skipEmptyRecords = true;
            internal char textQualifier = '"';
            internal bool trimWhitespace = true;
            internal bool useComments = false;
            internal bool useCustomRecordDelimiter = false;
            internal bool useTextQualifier = true;

            internal event CsvReader.UserSettings.SettingChangedEventHandler SettingChanged;

            internal UserSettings()
            {
            }

            private void ChangeSetting(CsvReader.UserSettings.Setting setting, object previousValue, object newValue)
            {
                if (this.SettingChanged == null || previousValue == newValue)
                    return;
                this.SettingChanged(setting, previousValue, newValue);
            }

            public bool CaptureRawRecord
            {
                get
                {
                    return this.captureRawRecord;
                }
                set
                {
                    this.ChangeSetting(CsvReader.UserSettings.Setting.CaptureRawRecord, (object)this.captureRawRecord, (object)value);
                    this.captureRawRecord = value;
                }
            }

            public bool CaseSensitive
            {
                get
                {
                    return this.caseSensitive;
                }
                set
                {
                    this.ChangeSetting(CsvReader.UserSettings.Setting.CaseSensitive, (object)this.caseSensitive, (object)value);
                    this.caseSensitive = value;
                }
            }

            public char Comment
            {
                get
                {
                    return this.comment;
                }
                set
                {
                    this.ChangeSetting(CsvReader.UserSettings.Setting.Comment, (object)this.comment, (object)value);
                    this.comment = value;
                }
            }

            public char Delimiter
            {
                get
                {
                    return this.delimiter;
                }
                set
                {
                    this.ChangeSetting(CsvReader.UserSettings.Setting.Delimiter, (object)this.delimiter, (object)value);
                    this.delimiter = value;
                }
            }

            public EscapeMode EscapeMode
            {
                get
                {
                    return this.escapeMode;
                }
                set
                {
                    this.ChangeSetting(CsvReader.UserSettings.Setting.EscapeMode, (object)this.escapeMode, (object)value);
                    this.escapeMode = value;
                }
            }

            public char RecordDelimiter
            {
                get
                {
                    return this.recordDelimiter;
                }
                set
                {
                    this.ChangeSetting(CsvReader.UserSettings.Setting.RecordDelimiter, (object)this.recordDelimiter, (object)value);
                    this.useCustomRecordDelimiter = true;
                    this.recordDelimiter = value;
                }
            }

            public bool SafetySwitch
            {
                get
                {
                    return this.safetySwitch;
                }
                set
                {
                    this.ChangeSetting(CsvReader.UserSettings.Setting.SafetySwitch, (object)this.safetySwitch, (object)value);
                    this.safetySwitch = value;
                }
            }

            public bool SkipEmptyRecords
            {
                get
                {
                    return this.skipEmptyRecords;
                }
                set
                {
                    this.ChangeSetting(CsvReader.UserSettings.Setting.SkipEmptyRecords, (object)this.skipEmptyRecords, (object)value);
                    this.skipEmptyRecords = value;
                }
            }

            public char TextQualifier
            {
                get
                {
                    return this.textQualifier;
                }
                set
                {
                    this.ChangeSetting(CsvReader.UserSettings.Setting.TextQualifier, (object)this.textQualifier, (object)value);
                    this.textQualifier = value;
                }
            }

            public bool TrimWhitespace
            {
                get
                {
                    return this.trimWhitespace;
                }
                set
                {
                    this.ChangeSetting(CsvReader.UserSettings.Setting.TrimWhitespace, (object)this.trimWhitespace, (object)value);
                    this.trimWhitespace = value;
                }
            }

            public bool UseComments
            {
                get
                {
                    return this.useComments;
                }
                set
                {
                    this.ChangeSetting(CsvReader.UserSettings.Setting.UseComments, (object)this.useComments, (object)value);
                    this.useComments = value;
                }
            }

            public bool UseTextQualifier
            {
                get
                {
                    return this.useTextQualifier;
                }
                set
                {
                    this.ChangeSetting(CsvReader.UserSettings.Setting.UseTextQualifier, (object)this.useTextQualifier, (object)value);
                    this.useTextQualifier = value;
                }
            }

            internal enum Setting
            {
                CaseSensitive,
                TextQualifier,
                TrimWhitespace,
                UseTextQualifier,
                Delimiter,
                RecordDelimiter,
                Comment,
                UseComments,
                EscapeMode,
                SafetySwitch,
                SkipEmptyRecords,
                CaptureRawRecord,
            }

            internal delegate void SettingChangedEventHandler(CsvReader.UserSettings.Setting setting, object previousValue, object newValue);
        }
    }
}
