using System.ComponentModel.DataAnnotations;

namespace Models
{
  public class UserForgotPasswordModel
  {
    [Required]
    [EmailAddress]
    public string Email { get; set; }
  }
}
