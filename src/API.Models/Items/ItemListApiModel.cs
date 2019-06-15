using System.ComponentModel.DataAnnotations;

namespace API.Models
{
  public class ItemListApiModel
  {
    public int Id { get; set; }
    public bool IsDone { get; set; }
    public int StatusId { get; set; }

    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string Text { get; set; }

    public int Priority { get; set; }
  }
}
