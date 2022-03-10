using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.ViewModels
{
    public class CreateIdeaViewModel
    {
        [Required]
        public string? Title { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string? Content { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ValidateNever]
        [NotMapped]
        public List<SelectListItem>? Categories { get; set; }

        [BooleanMustBeTrue(ErrorMessage = "You must Agree for Term and Conditions")]
        public bool IsCheckTerm { get; set; }
    }

    public class BooleanMustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return value is bool boolean && boolean;
        }
    }
}
