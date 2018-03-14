using log4net.Config;
using System;
using System.Diagnostics;
using System.IO;

namespace Log4netUtility
{
    internal class Log4netConfiguration
    {
        public static void Setup()
        {
            if (log4net.GlobalContext.Properties["RelatedPath"] == null)
            {
                log4net.GlobalContext.Properties["RelatedPath"] = string.Empty;
            }
            if (log4net.GlobalContext.Properties["ProcessName"] == null)
            {
                log4net.GlobalContext.Properties["ProcessName"] = Process.GetCurrentProcess().ProcessName + ".exe";
            }
            if (log4net.GlobalContext.Properties["LogFilePostfix"] == null)
            {
                log4net.GlobalContext.Properties["LogFilePostfix"] = string.Empty;
            }

            var log4netConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log4net.config");
            if (File.Exists(log4netConfig))
            {
                XmlConfigurator.ConfigureAndWatch(new FileInfo(log4netConfig));
            }
            else
            {
                if (System.Configuration.ConfigurationManager.GetSection("log4net") != null)
                {
                    XmlConfigurator.Configure();
                }
            }
        }
    }
}
