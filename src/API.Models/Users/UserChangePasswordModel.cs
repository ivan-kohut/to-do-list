using System.ComponentModel.DataAnnotations;

namespace API.Models
{
  public class UserChangePasswordModel
  {
    [Required]
    public string OldPassword { get; set; }

    [Required]
    public string NewPassword { get; set; }

    [Required]
    [Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; set; }
  }
}
