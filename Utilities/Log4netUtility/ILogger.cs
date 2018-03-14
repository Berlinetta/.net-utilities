namespace Log4netUtility
{
    public interface ILogger
    {
        void Error(string formatStr, params object[] args);
        void Warn(string formatStr, params object[] args);
        void Info(string formatStr, params object[] args);
        void Debug(string formatStr, params object[] args);
        void SetLogLevel(string level);
        void AddPostfix(string postFix);
    }
}
