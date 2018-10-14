using System.ComponentModel.DataAnnotations;

namespace Controllers.Models
{
  public class ItemApiModel
  {
    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string Text { get; set; }
  }
}
