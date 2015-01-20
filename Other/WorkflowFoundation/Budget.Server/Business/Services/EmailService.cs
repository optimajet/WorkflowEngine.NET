using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Transactions;
using Budget2.DAL;
using Budget2.Server.Business.Interface.Services;
using Common;

namespace Budget2.Server.Business.Services
{
    public class EmailService : Budget2DataContextService, IEmailService
    {
        private class MessageTemplate
        {
            public string Head { get; set; }
            public string Body { get; set; }
        }

        public EmailService ()
        {
            _timer = new Timer(ResetOnTimer);
        }

        private static ReaderWriterLock _lock = new ReaderWriterLock();

        private static int _timeout = 60000;

        private Dictionary<string, MessageTemplate> _templates;

        private Timer _timer;

        private void ResetOnTimer(object state)
        {
            //_timer.Change(Timeout.Infinite, Timeout.Infinite); //Останов таймера
            _lock.AcquireWriterLock(_timeout);
            try
            {
                _templates = null;
            }
            finally 
            {
                
                _lock.ReleaseWriterLock();
            }
        }

        private Dictionary<string, MessageTemplate> Templates
        {
            get
            {
               if (_templates == null || _templates.Count == 0)
                    {
                        LockCookie lc = _lock.UpgradeToWriterLock(_timeout);
                        try
                        {
                            _templates = new Dictionary<string, MessageTemplate>();
                            FillTemplates();
                            _timer.Change(new TimeSpan(0, 30, 0),new TimeSpan(-1));
                            
                        }
                        finally
                        {
                            _lock.DowngradeFromWriterLock(ref lc);
                        }
                    }

                return _templates;
                
            }
        }

        private MessageTemplate GetTemplate(string code)
        {
            MessageTemplate template;

            _lock.AcquireReaderLock(_timeout);
            try
            {
                template = Templates[code];
            }
            finally
            {
               _lock.ReleaseReaderLock();
            }

            return template;
        }

        private void FillTemplates ()
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = CreateContext())
                {
                    var templates = context.EMailTemplates.Select(p => p);
                    foreach (var eMailTemplate in templates)
                    {
                        _templates.Add(eMailTemplate.Code,
                                       new MessageTemplate() {Body = eMailTemplate.Body, Head = eMailTemplate.Head});
                    }
                }
            }
        }

        public void SendEmail(string templateCode, Dictionary<string, string> parameters)
        {
            SendEmail(templateCode, parameters, UPKZMailAddressTo);
        }

        public void SendEmail(string templateCode, Dictionary<string, string> parameters, string sendTo)
        {
            var template = GetTemplate(templateCode);

            if (template == null)
            {
                Logger.Log.ErrorFormat("E-mail:Не найден шаблон {0}", templateCode);
                return;
            }

            var sb = new StringBuilder(template.Body);
            var sbHead = new StringBuilder(template.Head);
            foreach (var kvp in parameters)
            {
                sbHead.Replace(kvp.Key, kvp.Value);
                sb.Replace(kvp.Key, kvp.Value);
            }
            var body = sb.ToString();
            var head = sbHead.ToString();
            if (string.IsNullOrEmpty(SenderEmail))
            {
                Logger.Log.Error("E-mail:Не задано значение SenderEmail");
                return;
            }


            if (string.IsNullOrEmpty(sendTo))
            {
                Logger.Log.Error("E-mail:Не задано значение UPKZMailAddressTo");
                return;
            }

            if (string.IsNullOrEmpty(SmtpServer))
            {
                Logger.Log.Error("E-mail:Не задано значение SmtpServer");
                return;
            }

            if (SmtpServerPort <= 0)
            {
                Logger.Log.Error("E-mail:Не задано значение SmtpServerPort");
                return;
            }



            body = string.Format("<html><body>{0}</body></html>", body);
            var message = new MailMessage(SenderEmail, sendTo, head, body);
            message.IsBodyHtml = true;
            var client = GetClient();
            client.SendAsync(message, null);
        }

        private SmtpClient GetClient ()
        {
            if (string.IsNullOrEmpty(SmtpServerUserName) || string.IsNullOrEmpty(SmtpServerPassword))
                return new SmtpClient(SmtpServer, SmtpServerPort);

            var client = new SmtpClient(SmtpServer, SmtpServerPort)
                             {
                                 Credentials =
                                     string.IsNullOrEmpty(SmtpServerUserDomain)
                                         ? new NetworkCredential(SmtpServerUserName,
                                                                 SmtpServerPassword)
                                         : new NetworkCredential(SmtpServerUserName,
                                                                 SmtpServerPassword, SmtpServerUserDomain)
            };
            return client;
        }

        private string SmtpServer
        {
            get { return ConfigurationManager.AppSettings.Get("SmtpServer"); }
        }

        private string SmtpServerUserName
        {
            get { return ConfigurationManager.AppSettings.Get("SmtpServerUserName"); }
        }

        private string SmtpServerUserDomain
        {
            get { return ConfigurationManager.AppSettings.Get("SmtpServerUserDomain"); }
        }

        private int SmtpServerPort
        {
            get
            {
                int port = -1;
                int.TryParse(ConfigurationManager.AppSettings.Get("SmtpServerPort"),out port);
                return port;
            }
        }

        private string SenderEmail
        {
            get { return ConfigurationManager.AppSettings.Get("SenderEmail"); }
        }

        private string SmtpServerPassword
        {
            get { return ConfigurationManager.AppSettings.Get("SmtpServerPassword"); }
        }

       
        private string UPKZMailAddressTo
        {
            get { return ConfigurationManager.AppSettings.Get("UPKZMailAddressTo"); }
        }
    }
}
