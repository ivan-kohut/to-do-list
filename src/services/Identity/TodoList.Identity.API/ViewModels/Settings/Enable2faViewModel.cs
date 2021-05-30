using System.ComponentModel.DataAnnotations;

namespace TodoList.Identity.API.ViewModels
{
  public class Enable2faViewModel
  {
    [Required]
    [RegularExpression("[0-9]{6}")]
    public string? Code { get; set; }

    public string? AuthenticatorUri { get; set; }
  }
}
