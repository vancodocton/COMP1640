using System.ComponentModel.DataAnnotations;
using App.Core.Entities;

namespace App.Web.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public IEnumerable<Idea>? Ideas { get; set; }

        [Display(Name = "Due Date")]
        [DataType(DataType.DateTime)]
        [Required]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Final Due Date")]
        [DataType(DataType.DateTime)]
        [Required]
        public DateTime? FinalDueDate { get; set; }
    }
}
