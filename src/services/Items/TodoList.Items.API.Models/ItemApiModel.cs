using System.ComponentModel.DataAnnotations;

namespace TodoList.Items.API.Models
{
    public class ItemApiModel
    {
        public int Id { get; set; }

        public bool IsDone { get; set; }

        [Required]
        [StringLength(128, MinimumLength = 1)]
        public string Text { get; set; } = null!;

        public int Priority { get; set; }
    }
}
