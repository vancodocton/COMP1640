using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace App.Web.ViewModels
{
    public class AccountUpdateViewModel
    {
        public string? Id { get; set; }

        [Required]
        public string? Email { get; set; }

        [StringLength(50)]
        [Required]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? BirthDate { get; set; }

        [StringLength(80)]
        public string? Address { get; set; }

        [Required]
        public string? Role { get; set; } = string.Empty;

        [ValidateNever]
        public List<SelectListItem>? Roles { get; set; }

        [Display(Name = "Department")]
        [Required]
        public int? DepartmentId { get; set; }
        [ValidateNever]
        public List<SelectListItem>? Departments { get; set; }
    }
}

