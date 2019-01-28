using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Services
{
  public class EmailService : IEmailService
  {
    private readonly string sendGridApiKey;

    public EmailService(string sendGridApiKey)
    {
      this.sendGridApiKey = sendGridApiKey;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
      SendGridMessage msg = new SendGridMessage
      {
        From = new EmailAddress("todo-list@app.com"),
        Subject = subject,
        PlainTextContent = message,
        HtmlContent = message
      };

      msg.AddTo(new EmailAddress(email));

      msg.SetClickTracking(false, false);

      await new SendGridClient(sendGridApiKey).SendEmailAsync(msg);
    }
  }
}
