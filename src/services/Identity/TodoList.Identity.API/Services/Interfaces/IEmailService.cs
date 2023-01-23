using System.Threading.Tasks;

namespace TodoList.Identity.API.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
