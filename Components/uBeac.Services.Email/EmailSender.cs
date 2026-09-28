/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace uBeac.Services.Email
{
    public class EmailSender : IEmailSender
    {
        MailServerSettings _mailServerSettings;
        SmtpClient _smtpClient;

        public EmailSender(IConfiguration configuration)
        {
            _mailServerSettings = configuration.GetSection("MailServerSettings").Get<MailServerSettings>();
            Init();
        }

        private void Init()
        {
            _smtpClient = new SmtpClient(_mailServerSettings.Host, _mailServerSettings.Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_mailServerSettings.Username, _mailServerSettings.Password),
                EnableSsl = _mailServerSettings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(_mailServerSettings.SenderEmail, _mailServerSettings.SenderDisplayName),
                Body = message,
                IsBodyHtml = true,
                Subject = subject,
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.UTF8
            };
            mailMessage.To.Add(email);
            await _smtpClient.SendMailAsync(mailMessage);
        }
    }
}
