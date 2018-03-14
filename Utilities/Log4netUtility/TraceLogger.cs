using System;
using System.Diagnostics;

namespace Log4netUtility
{
    internal class TraceLogger : ILogger
    {
        public void Error(string formatStr, params object[] args)
        {
            try
            {
                Trace.WriteLine("Error: " + string.Format(formatStr, args));
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }
        }

        public void Warn(string formatStr, params object[] args)
        {
            try
            {
                Trace.WriteLine("Warn: " + string.Format(formatStr, args));
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }
        }

        public void Info(string formatStr, params object[] args)
        {
            try
            {
                Trace.WriteLine("Info: " + string.Format(formatStr, args));
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }
        }

        public void Debug(string formatStr, params object[] args)
        {
            try
            {
                Trace.WriteLine("Debug: " + string.Format(formatStr, args));
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }
        }

        public void SetLogLevel(string level)
        {

        }

        public void AddPostfix(string postFix)
        {

        }
    }
}
