using System.ComponentModel.DataAnnotations;

namespace TodoList.Items.API.Models
{
    public class ItemCreateApiModel
    {
        [Required]
        [StringLength(128, MinimumLength = 1)]
        public string Text { get; set; } = null!;
    }
}
