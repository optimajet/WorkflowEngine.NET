using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace ExternalServicesAPI
{
    internal sealed class Settings
    {
        public static string GetDocStatusUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["externalService.getDocStatusUrl"];
            }
        }

        public static string NewDocExportUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["externalService.newDocExportUrl"];
            }
        }
    }
}
