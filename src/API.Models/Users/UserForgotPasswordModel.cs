using System.ComponentModel.DataAnnotations;

namespace API.Models
{
  public class UserForgotPasswordModel
  {
    [Required]
    [EmailAddress]
    public string Email { get; set; }
  }
}
