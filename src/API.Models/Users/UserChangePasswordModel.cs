using System.ComponentModel.DataAnnotations;

namespace API.Models
{
  public class UserChangePasswordModel
  {
    [Required]
    [Display(Name = "Current")]
    public string OldPassword { get; set; } = null!;

    [Required]
    [Display(Name = "New")]
    public string NewPassword { get; set; } = null!;

    [Required]
    [Display(Name = "Retype new")]
    [Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; set; } = null!;
  }
}
