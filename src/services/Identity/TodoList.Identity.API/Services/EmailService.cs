using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using TodoList.Identity.API.Options;

namespace TodoList.Identity.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly SendGridOptions sendGridOptions;

        public EmailService(IOptions<SendGridOptions> sendGridOptions)
        {
            this.sendGridOptions = sendGridOptions.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            SendGridMessage msg = new()
            {
                From = new EmailAddress(sendGridOptions.SenderEmail),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };

            msg.AddTo(new EmailAddress(email));

            msg.SetClickTracking(false, false);

            await new SendGridClient(sendGridOptions.ApiKey).SendEmailAsync(msg);
        }
    }
}
