using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Contracts
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlMessage);
    }

}
