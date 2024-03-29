using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Bookify.Web.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly MailSetting _mailSetting;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public EmailSender(IOptions<MailSetting> mailSetting, IWebHostEnvironment webHostEnvironment)
        {
            _mailSetting = mailSetting.Value;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            MailMessage message = new MailMessage()
            {
                From = new MailAddress(_mailSetting.Email, _mailSetting.DisplayName),
                Body = htmlMessage,
                Subject = subject,
                IsBodyHtml = true
            };

            message.To.Add(_webHostEnvironment.IsDevelopment() ? "mostafasalem858@outlook.com" : email);

            SmtpClient smtp = new(_mailSetting.Host)
            {
                Port = _mailSetting.Port,
                Credentials = new NetworkCredential(_mailSetting.Email, _mailSetting.Password),
                EnableSsl = true
            };
            await smtp.SendMailAsync(message);
            smtp.Dispose();
        }
    }
}
