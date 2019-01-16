using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Sample.Redis
{
    public class ConnectionSettingsProvider
    {
        public ConnectionSettingsProvider()
        {
            Multiplexer = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["ConnectionMultiplexerConfiguration"]);
            ProviderNamespace = ConfigurationManager.AppSettings["ProviderNamespace"];
        }

        public ConnectionMultiplexer Multiplexer { get; private set; }
        public string ProviderNamespace { get; private set; }
    }
}
