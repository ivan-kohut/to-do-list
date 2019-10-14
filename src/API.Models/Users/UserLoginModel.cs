using System.ComponentModel.DataAnnotations;

namespace API.Models
{
  public class UserLoginModel
  {
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    [RegularExpression("[0-9]{6}")]
    public string? TwoFactorToken { get; set; }
  }
}
