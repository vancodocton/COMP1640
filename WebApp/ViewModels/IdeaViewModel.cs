using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class IdeaViewModel
    {
        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string? Content { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ValidateNever]
        public List<SelectListItem>? Categories { get; set; }
    }
}
