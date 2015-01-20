using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.Server.Business.Interface.Services
{
    public interface IEmailService
    {
        void SendEmail(string templateCode, Dictionary<string, string> parameters);
        void SendEmail(string templateCode, Dictionary<string, string> parameters, string sendTo);
    }
}
