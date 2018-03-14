namespace Log4netUtility
{
    public class Logger
    {
        private static readonly object InitializeLock = new object();
        private static ILogger Local;

        public static ILogger Instance
        {
            get
            {
                if (Local == null)
                {
                    lock (InitializeLock)
                    {
                        if (Local == null)
                        {
                            Initialize();
                        }
                    }
                }
                return Local;
            }
        }

        private static void Initialize()
        {
            Log4netConfiguration.Setup();
            Local = new Log4netLogger();
        }
    }
}
