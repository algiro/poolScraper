using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtils.Mail
{
    public delegate ConfigurazioneSmtp GetConfigurazioneSmtp();
    public interface IMailService
    {
        void Send(string subject, string body, string to = null);
    }

    public class MailService : IMailService
    {
        private ConfigurazioneSmtp configurazioneSmtp;

        public MailService(GetConfigurazioneSmtp getConfigurazioneSmtp)
        {
            configurazioneSmtp = getConfigurazioneSmtp();
        }

        public void Send(string subject, string body, string to = null)
        {
            using (var client = BuildSmtpClient())
            {
                var destinatario = !string.IsNullOrWhiteSpace(to) ? to : configurazioneSmtp.EmailDestinatario;
                var mailMessage = CreaMessaggioMail(subject, body, destinatario);
                client.Send(mailMessage);
            }
        }

        private MailMessage CreaMessaggioMail(string subject, string body, string to)
        {
            var message = new MailMessage();
            message.To.Add(to);
            // message.CC.Add("it-fmufficiopersonal@kpmg.it");
            message.From = new MailAddress(configurazioneSmtp.EmailMittente);
            message.Body = body;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.Subject = subject;
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            message.IsBodyHtml = true;

            return message;
        }

        private SmtpClient BuildSmtpClient()
        {
            var client = new SmtpClient();
            client.Host = configurazioneSmtp.Host;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            return client;
        }
    }
}
