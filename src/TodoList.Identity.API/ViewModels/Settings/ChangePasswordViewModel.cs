using System.ComponentModel.DataAnnotations;

namespace TodoList.Identity.API.ViewModels
{
  public class ChangePasswordViewModel
  {
    [Required]
    public string? OldPassword { get; set; }

    [Required]
    public string? NewPassword { get; set; }

    [Required]
    [Compare(nameof(NewPassword))]
    public string? ConfirmNewPassword { get; set; }
  }
}
