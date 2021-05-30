using System.ComponentModel.DataAnnotations;

namespace TodoList.Identity.API.ViewModels
{
  public class LoginWith2faViewModel : ViewModelBase
  {
    [Required]
    [RegularExpression("[0-9]{6}")]
    public string? TwoFactorToken { get; set; }
  }
}
