using System.ComponentModel.DataAnnotations;

namespace Controllers.Models
{
  public class ItemCreateApiModel
  {
    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string Text { get; set; }
  }
}
