using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtils.Mail
{
    public class ConfigurazioneSmtp
    {
        public string Host { get; set; }

        public string Port { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public string Auth { get; set; }

        public string SslEnabled { get; set; }

        public string EmailMittente { get; set; }
        public string EmailDestinatario { get; set; }

        public ConfigurazioneSmtp()
        {
        }
    }

    public static class ConfigurazioneSmtpHelper
    {
        public static ConfigurazioneSmtp CreateConfigurazioneSmtp(this IConfiguration configuration)
        {
            var configurazioneSmtp = new ConfigurazioneSmtp();

            configurazioneSmtp.Host = configuration["SmtpSettings:Host"];
            configurazioneSmtp.Port = configuration["SmtpSettings:Port"];
            configurazioneSmtp.User = configuration["SmtpSettings:User"];
            configurazioneSmtp.Password = configuration["SmtpSettings:Password"];
            configurazioneSmtp.Auth = configuration["SmtpSettings:Auth"];
            configurazioneSmtp.SslEnabled = configuration["SmtpSettings:SslEnabled"];
            configurazioneSmtp.EmailMittente = configuration["SmtpSettings:EmailMittente"];
            configurazioneSmtp.EmailDestinatario = configuration["SmtpSettings:EmailDestinatario"];

            return configurazioneSmtp;
        }
    }
}
