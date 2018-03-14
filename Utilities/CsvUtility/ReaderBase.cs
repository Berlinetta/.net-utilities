using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace CsvUtility
{
    public abstract class ReaderBase : IDisposable, IEnumerator, IEnumerable
    {
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct StaticSettings
        {
            public const int InitialColumnCount = 10;
            public const string Demo = "DEMO";
        }
        protected int columnsCount = 0;
        protected ulong currentRecord = 0uL;
        protected bool disposed = false;
        protected string[] values = new string[10];
        public ulong CurrentRecord
        {
            get
            {
                return this.currentRecord - 1uL;
            }
        }
        public string this[int columnIndex]
        {
            get
            {
                this.CheckDisposed();
                string result;
                if (columnIndex > -1 && columnIndex < this.columnsCount)
                {
                    result = this.values[columnIndex];
                }
                else
                {
                    result = "";
                }
                return result;
            }
        }
        object IEnumerator.Current
        {
            get
            {
                return this.Values;
            }
        }
        public string[] Values
        {
            get
            {
                this.CheckDisposed();
                string[] array = new string[this.columnsCount];
                for (int i = 0; i < this.columnsCount; i++)
                {
                    array[i] = this[i];
                }
                return array;
            }
        }
        protected void CheckDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName, "This object has been previously disposed. Methods on this object can no longer be called.");
            }
        }
        public void Close()
        {
            this.Dispose(true);
        }
        protected abstract void Dispose(bool disposing);
        public abstract bool ReadRecord();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
        bool IEnumerator.MoveNext()
        {
            return this.ReadRecord();
        }
        void IEnumerator.Reset()
        {
            throw new NotSupportedException("Reset is not currently supported by the IEnumerable implementation supplied by " + base.GetType().FullName + ".");
        }
        void IDisposable.Dispose()
        {
            if (!this.disposed)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
