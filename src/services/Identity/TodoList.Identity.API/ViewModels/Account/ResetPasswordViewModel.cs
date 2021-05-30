using System.ComponentModel.DataAnnotations;

namespace TodoList.Identity.API.ViewModels
{
  public class ResetPasswordViewModel : ViewModelBase
  {
    [Required]
    public string? Password { get; set; }

    [Required]
    [Compare(nameof(Password))]
    public string? ConfirmPassword { get; set; }

    public int? Id { get; set; }

    public string? Code { get; set; }
  }
}
