using log4net;
using System;

namespace Log4netUtility
{
    internal class Log4netLogger : ILogger
    {
        private TraceLogger traceLogger = new TraceLogger();
        private string postFix = string.Empty;

        private ILog log4netLogger;

        public Log4netLogger()
        {
            log4netLogger = LogManager.GetLogger(string.Empty);
        }

        public void Error(string formatStr, params object[] args)
        {
            if (log4netLogger.IsErrorEnabled)
            {
                try
                {
                    log4netLogger.Error(string.Format(formatStr, args) + " " + postFix);
                }
                catch (Exception e)
                {
                    traceLogger.Error(e.Message);
                }
            }
        }

        public void Warn(string formatStr, params object[] args)
        {
            if (log4netLogger.IsWarnEnabled)
            {
                try
                {
                    log4netLogger.Warn(string.Format(formatStr, args) + " " + postFix);
                }
                catch (Exception e)
                {
                    traceLogger.Error(e.Message);
                }
            }
        }

        public void Info(string formatStr, params object[] args)
        {
            if (log4netLogger.IsInfoEnabled)
            {
                try
                {
                    log4netLogger.Info(string.Format(formatStr, args) + " " + postFix);
                }
                catch (Exception e)
                {
                    traceLogger.Error(e.Message);
                }
            }
        }

        public void Debug(string formatStr, params object[] args)
        {
            if (log4netLogger.IsDebugEnabled)
            {
                try
                {
                    log4netLogger.Debug(string.Format(formatStr, args) + " " + postFix);
                }
                catch (Exception e)
                {
                    traceLogger.Error(e.Message);
                }
            }
        }


        public void SetLogLevel(string level)
        {
            log4net.Repository.ILoggerRepository[] repositories = log4net.LogManager.GetAllRepositories();
            //Configure all loggers to be at the debug level.
            foreach (log4net.Repository.ILoggerRepository repository in repositories)
            {
                repository.Threshold = repository.LevelMap[level];
                log4net.Repository.Hierarchy.Hierarchy hier = (log4net.Repository.Hierarchy.Hierarchy)repository;
                log4net.Core.ILogger[] loggers = hier.GetCurrentLoggers();
                foreach (log4net.Core.ILogger logger in loggers)
                {
                    ((log4net.Repository.Hierarchy.Logger)logger).Level = hier.LevelMap[level];
                }
            }
            //Configure the root logger.
            log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            log4net.Repository.Hierarchy.Logger rootLogger = h.Root;
            rootLogger.Level = h.LevelMap[level];

        }


        public void AddPostfix(string postFix)
        {
            this.postFix = postFix;
        }
    }
}
