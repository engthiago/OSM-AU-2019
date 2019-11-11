using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Osm.Web.Services
{
    public class MailService
    {
        private readonly IConfiguration config;

        public MailService(IConfiguration config)
        {
            this.config = config;
        }

        public async Task SendWorkCompleteEmail(string email, string downloadURL, string viewerUrl)
        {
            var body = $"<h1>Job Processing Complete</h1><h3>Please download the results <a href='{downloadURL}'>here</a></h3><h3>You can view the results <a href='{viewerUrl}'>here</a><br><br><p><i>*These links will be available for 24 hours.</i></p></h3>";
            var message = this.CreateMessage("Design Automation: Job Completed", body);
            message.To.Add(new MailboxAddress(email));

            await this.SendAsync(message);
        }

        private MimeMessage CreateMessage(string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Design Automation", config["Email"]));

            message.Subject = subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = body;

            message.Body = builder.ToMessageBody();

            return message;
        }

        private async Task SendAsync(MimeMessage message)
        {
            using (var eClient = new SmtpClient())
            {
                eClient.Connect("smtp.office365.com", 587, false);
                eClient.Authenticate(config["Email"], config["EmailPass"]);
                await eClient.SendAsync(message);
            }
        }
    }
}
