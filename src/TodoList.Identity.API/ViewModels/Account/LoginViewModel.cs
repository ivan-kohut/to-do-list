using System.ComponentModel.DataAnnotations;

namespace TodoList.Identity.API.ViewModels
{
  public class LoginViewModel
  {
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? Password { get; set; }

    public string? ReturnUrl { get; set; }
  }
}
