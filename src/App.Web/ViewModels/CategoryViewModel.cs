using System.ComponentModel.DataAnnotations;
using App.Web.Models;

namespace App.Web.ViewModels
{
    public class CategoryViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

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
