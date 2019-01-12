using System.ComponentModel.DataAnnotations;

namespace Controllers.Models
{
  public class ItemUpdateApiModel
  {
    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string Text { get; set; }

    [Required]
    public int Priority { get; set; }
  }
}
