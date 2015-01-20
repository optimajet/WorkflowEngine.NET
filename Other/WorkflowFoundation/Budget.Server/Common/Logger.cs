using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Config;


namespace Common
{
    public static class Logger
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Logger));
        private static bool _configured;
        private static volatile object _lock = new object();
        public static ILog Log
        {
            get
            {
                if (!_configured)
                    lock (_lock)
                    {
                        if (!_configured)
                        {
                            InitLogger();
                            _configured = true;
                        }
                    }
                return log;
            }
        }

        public static void InitLogger()
        {
            XmlConfigurator.Configure();
        }
    }
}
