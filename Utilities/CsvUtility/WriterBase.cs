using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
namespace CsvUtility
{
    public abstract class WriterBase : IDisposable
    {
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct StaticSettings
        {
            public const int MaxFileBufferSize = 4096;
        }
        protected bool disposed = false;
        protected Encoding encoding = null;
        protected string fileName = null;
        protected bool initialized = false;
        protected TextWriter outputStream = null;
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
        public void Flush()
        {
            this.outputStream.Flush();
        }
        protected void Init()
        {
            if (!this.initialized)
            {
                if (this.fileName != null)
                {
                    this.outputStream = new StreamWriter(new FileStream(this.fileName, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, false), this.encoding);
                }
                this.initialized = true;
            }
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
