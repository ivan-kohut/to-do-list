using System.ComponentModel.DataAnnotations;

namespace API.Models
{
  public class UserEnableAuthenticatorModel
  {
    [Required]
    [RegularExpression("[0-9]{6}")]
    public string Code { get; set; }
  }
}
