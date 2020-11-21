using System.ComponentModel.DataAnnotations;

namespace TodoList.Identity.API.ViewModels
{
  public class ForgotPasswordViewModel : ViewModelBase
  {
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
  }
}
