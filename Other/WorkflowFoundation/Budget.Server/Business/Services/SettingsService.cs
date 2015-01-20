using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Budget2.Server.Business.Interface.Services;

namespace Budget2.Server.Business.Services
{
    public class SettingsService : ISettingsService
    {
        public string HeadFilialCode
        {
            get { return ConfigurationManager.AppSettings.Get("HeadFilialCode"); }
        }
    }
}
