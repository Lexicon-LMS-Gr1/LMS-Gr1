using Microsoft.Extensions.Configuration;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace LMS.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration config;

        public EmailService(IConfiguration config)
        {
            this.config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            var smtp = new SmtpClient
            {
                Host = config["Email:SmtpHost"],
                Port = int.Parse(config["Email:SmtpPort"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    config["Email:Username"],
                    config["Email:Password"]
                )
            };

            var mail = new MailMessage
            {
                From = new MailAddress(config["Email:From"]),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mail.To.Add(to);

            await smtp.SendMailAsync(mail);
        }
    }

}
