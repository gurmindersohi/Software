using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

namespace Sohi.Web.Services.Emails
{
    public class EmailSender : IEmailSender
    {

        private IConfiguration _config;

        public EmailSender(IConfiguration configuration)
        {
            _config = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(subject, message, email);
        }

        public async Task Execute(string subject, string message, string email)
        {
            string Email = _config.GetSection("noreplyEmailCredentials").GetSection("Email").Value;
            string Password = _config.GetSection("noreplyEmailCredentials").GetSection("Password").Value;

            MailMessage mail = new MailMessage();

            mail.From = new MailAddress(Email);
            mail.To.Add(email);
            mail.Subject = subject;
            mail.Body = message;
            mail.IsBodyHtml = true;

            SmtpClient client = new SmtpClient("smtp.gmail.com");

            client.Port = 587;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(Email, Password);

            await client.SendMailAsync(mail);

        }

    }
}
