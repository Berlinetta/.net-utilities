namespace DALUtility
{
    using System;
    using System.Configuration;

    public static class FundamentalsConfiguration
    {
        public static int CommandTimeout
        {
            get
            {
                string commandTimeout = ConfigurationManager.AppSettings["commandTimeout"];
                if (string.IsNullOrEmpty(commandTimeout))
                {
                    return 300;
                }
                else
                {
                    return int.Parse(commandTimeout);
                }
            }
        }
    }
}