using System;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CsvUtility
{
    public sealed class CsvWriter : WriterBase
    {
        private bool firstColumn;
        private CsvWriter.UserSettings userSettings;

        public CsvWriter(string fileName)
          : this(fileName, ',', Encoding.Default)
        {
        }

        public CsvWriter(TextWriter outputStream, char delimiter)
        {
            this.firstColumn = true;
            this.userSettings = new CsvWriter.UserSettings();
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream), "Output stream can not be null.");
            this.outputStream = outputStream;
            this.userSettings.delimiter = delimiter;
            this.initialized = true;
        }

        public CsvWriter(Stream outputStream, char delimiter, Encoding encoding)
          : this((TextWriter)new StreamWriter(outputStream, encoding), delimiter)
        {
        }

        public CsvWriter(string fileName, char delimiter, Encoding encoding)
        {
            this.firstColumn = true;
            this.userSettings = new CsvWriter.UserSettings();
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName), "File name can not be null.");
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding), "Encoding can not be null.");
            this.fileName = fileName;
            this.userSettings.delimiter = delimiter;
            this.encoding = encoding;
            this.Init();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
                return;
            if (disposing)
                this.encoding = (Encoding)null;
            if (this.initialized)
                this.outputStream.Dispose();
            this.outputStream = (TextWriter)null;
            this.disposed = true;
        }

        public void EndRecord()
        {
            this.CheckDisposed();
            if (this.userSettings.useCustomRecordDelimiter)
                this.outputStream.Write(this.userSettings.recordDelimiter);
            else
                this.outputStream.WriteLine();
            this.firstColumn = true;
        }

        ~CsvWriter()
        {
            this.Dispose(false);
        }

        public void Write(string content)
        {
            this.Write(content, false);
        }

        public void Write(string content, bool preserveSpaces)
        {
            this.CheckDisposed();
            if (content == null)
                content = "";
            if (!this.firstColumn)
                this.outputStream.Write(this.userSettings.delimiter);
            bool flag = this.userSettings.forceQualifier;
            if (!preserveSpaces && content.Length > 0)
                content = content.Trim(' ', '\t');
            int num;
            if (!flag && this.userSettings.useTextQualifier)
            {
                if (!this.userSettings.useCustomRecordDelimiter)
                {
                    if (content.IndexOfAny(new char[4]
                    {
            '\n',
            '\r',
            this.userSettings.textQualifier,
            this.userSettings.delimiter
                    }) > -1)
                        goto label_12;
                }
                if (this.userSettings.useCustomRecordDelimiter)
                {
                    if (content.IndexOfAny(new char[3]
                    {
            this.userSettings.recordDelimiter,
            this.userSettings.textQualifier,
            this.userSettings.delimiter
                    }) > -1)
                        goto label_12;
                }
                num = !this.firstColumn || content.Length <= 0 || (int)content[0] != (int)this.userSettings.comment ? (!this.firstColumn ? 1 : (content.Length != 0 ? 1 : 0)) : 0;
                goto label_14;
                label_12:
                num = 0;
            }
            else
                num = 1;
            label_14:
            if (num == 0)
                flag = true;
            if (this.userSettings.useTextQualifier && !flag && (content.Length > 0 && preserveSpaces))
            {
                switch (content[0])
                {
                    case '\t':
                    case ' ':
                        flag = true;
                        break;
                }
                if (!flag && content.Length > 1)
                {
                    switch (content[content.Length - 1])
                    {
                        case '\t':
                        case ' ':
                            flag = true;
                            break;
                    }
                }
            }
            char ch1;
            if (flag)
            {
                this.outputStream.Write(this.userSettings.textQualifier);
                if (this.userSettings.escapeMode == EscapeMode.Backslash)
                {
                    if (content.IndexOf('\\') > -1)
                    {
                        ch1 = '\\';
                        char ch2 = '\\';
                        content = content.Replace(ch2.ToString(), ch2.ToString() + (object)'\\');
                    }
                    if (content.IndexOf(this.userSettings.textQualifier) > -1)
                        content = content.Replace(this.userSettings.textQualifier.ToString(), '\\'.ToString() + (object)this.userSettings.textQualifier);
                }
                else if (content.IndexOf(this.userSettings.textQualifier) > -1)
                    content = content.Replace(this.userSettings.textQualifier.ToString(), this.userSettings.textQualifier.ToString() + (object)this.userSettings.textQualifier);
            }
            else if (this.userSettings.escapeMode == EscapeMode.Backslash)
            {
                if (content.IndexOf('\\') > -1)
                {
                    ch1 = '\\';
                    char ch2 = '\\';
                    content = content.Replace(ch2.ToString(), ch2.ToString() + (object)'\\');
                }
                if (content.IndexOf(this.userSettings.delimiter) > -1)
                    content = content.Replace(this.userSettings.delimiter.ToString(), '\\'.ToString() + (object)this.userSettings.delimiter);
                if (this.userSettings.useCustomRecordDelimiter)
                {
                    if (content.IndexOf(this.userSettings.recordDelimiter) > -1)
                        content = content.Replace(this.userSettings.recordDelimiter.ToString(), '\\'.ToString() + (object)this.userSettings.recordDelimiter);
                }
                else
                {
                    if (content.IndexOf('\r') > -1)
                    {
                        ch1 = '\r';
                        char ch2 = '\\';
                        content = content.Replace(ch2.ToString(), ch2.ToString() + (object)'\r');
                    }
                    if (content.IndexOf('\n') > -1)
                    {
                        ch1 = '\n';
                        char ch2 = '\\';
                        content = content.Replace(ch2.ToString(), ch2.ToString() + (object)'\n');
                    }
                }
                if (this.firstColumn && content.Length > 0 && (int)content[0] == (int)this.userSettings.comment)
                    content = content.Length <= 1 ? '\\'.ToString() + (object)this.userSettings.comment : '\\'.ToString() + (object)this.userSettings.comment + content.Substring(1);
            }
            this.outputStream.Write(content);
            if (flag)
                this.outputStream.Write(this.userSettings.textQualifier);
            this.firstColumn = false;
        }

        public void WriteAll(DataTable data)
        {
            this.WriteAll(data, true);
        }

        public void WriteAll(DataTable data, bool writeHeaders)
        {
            if (data == null)
                return;
            if (writeHeaders)
            {
                foreach (DataColumn column in (InternalDataCollectionBase)data.Columns)
                    this.Write(column.ColumnName);
                this.EndRecord();
            }
            int count1 = data.Columns.Count;
            int count2 = data.Rows.Count;
            foreach (DataRow row in (InternalDataCollectionBase)data.Rows)
            {
                for (int index = 0; index < count1; ++index)
                    this.Write(row[index].ToString());
                this.EndRecord();
            }
            this.outputStream.Flush();
        }

        public void WriteComment(string commentText)
        {
            this.CheckDisposed();
            this.outputStream.Write(this.userSettings.comment);
            this.outputStream.Write(commentText);
            if (this.userSettings.useCustomRecordDelimiter)
                this.outputStream.Write(this.userSettings.recordDelimiter);
            else
                this.outputStream.WriteLine();
            this.firstColumn = true;
        }

        public void WriteRecord(string[] values)
        {
            this.WriteRecord(values, false);
        }

        public void WriteRecord(string[] values, bool preserveSpaces)
        {
            if (values == null || values.Length <= 0)
                return;
            foreach (string content in values)
                this.Write(content, preserveSpaces);
            this.EndRecord();
        }

        public CsvWriter.UserSettings Settings
        {
            get
            {
                return this.userSettings;
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
        }

        public class UserSettings
        {
            internal char comment = '#';
            internal char delimiter = ',';
            internal EscapeMode escapeMode = EscapeMode.Doubled;
            internal bool forceQualifier = false;
            internal char recordDelimiter = char.MinValue;
            internal char textQualifier = '"';
            internal bool useCustomRecordDelimiter = false;
            internal bool useTextQualifier = true;

            internal UserSettings()
            {
            }

            public char Comment
            {
                get
                {
                    return this.comment;
                }
                set
                {
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
                    this.escapeMode = value;
                }
            }

            public bool ForceQualifier
            {
                get
                {
                    return this.forceQualifier;
                }
                set
                {
                    this.forceQualifier = value;
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
                    this.useCustomRecordDelimiter = true;
                    this.recordDelimiter = value;
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
                    this.textQualifier = value;
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
                    this.useTextQualifier = value;
                }
            }
        }
    }
}
