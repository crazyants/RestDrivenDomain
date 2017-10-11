﻿using RDD.Domain;
using System;
using System.Net;
using System.Net.Mail;

namespace RDD.Infra.Mails
{
    public class SmtpMailService : IMailService
    {
        private readonly SmtpServerInfo _serverInfo;
        private readonly ExceptionMailInfo _exceptionMailInfo;
        private readonly IWebContext _webContext;

        public SmtpMailService(SmtpServerInfo serverInfo, ExceptionMailInfo exceptionMailInfo, IWebContext webContext)
        {
            _serverInfo = serverInfo;
            _exceptionMailInfo = exceptionMailInfo;
            _webContext = webContext;
        }

        public void SendMail(string from, string to, string subject, string body, bool forceSend = false)
        {
            try
            {
                if (forceSend)
                {
                    var mail = new MailMessage
                    {
                        From = new MailAddress(from),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mail.To.Add(to);

                    using (var smtpClient = new SmtpClient())
                    {
                        smtpClient.Host = _serverInfo.Server;
                        smtpClient.Port = _serverInfo.Port;
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.EnableSsl = _serverInfo.Ssl;

                        if (!String.IsNullOrEmpty(_serverInfo.Login))
                        {
                            smtpClient.UseDefaultCredentials = false;
                            smtpClient.Credentials = new NetworkCredential(_serverInfo.Login, _serverInfo.Password);
                        }

                        smtpClient.Send(mail);
                    };

                    return;
                }
            }
            catch (Exception e)
            {
            }
        }

        public void SendExceptionMail(Exception e)
        {
            try
            {
                var stackTrace = String.Empty;

                //Si l'Exception est instanciée à la mano, y'a pas de StackTrace
                if (e.StackTrace != null)
                {
                    stackTrace = e.StackTrace.Replace("\r\n", "<br />");
                }

                var body = String.Format("Erreur : {0}<br /><br />{1}<br /><br />", e.Message, stackTrace);

                if (e.InnerException != null)
                {
                    var innerStackTrace = String.Empty;

                    if (e.InnerException.StackTrace != null)
                    {
                        innerStackTrace = e.InnerException.StackTrace.Replace("\r\n", "<br />");
                    }

                    body += String.Format("Inner : {0}<br /><br />{1}<br /><br />", e.InnerException.Message, innerStackTrace);
                }

                try
                {
                    var Detail = new Func<string, string, string>((k, v) => String.Format("{0} : {1}<br /><br />", k, v));

                    body += Detail("Page REST", _webContext.RawUrl);
                    body += Detail("Page", _webContext.Url.ToString());
                    body += Detail("Host Address", _webContext.UserHostAddress);
                }
                catch { } // No context

                SendMail(_exceptionMailInfo.Sender, _exceptionMailInfo.Recipient, _exceptionMailInfo.Subject, body, true);
            }
            catch { }
        }
        public void SendCriticalExceptionMail(string message)
        {
            var body = $"Critical log level reached : {message}";

            SendMail(_exceptionMailInfo.Sender, _exceptionMailInfo.Recipient, _exceptionMailInfo.Subject, body, true);
        }
    }
}
