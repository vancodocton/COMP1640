using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WebApp.Areas.Identity.Pages.Account;

namespace WebApp.ViewModels
{
    public class AccountCreateViewModel
    {
        public RegisterModel.InputModel Input { get; set; } = new();

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
        public string Role { get; set; } = string.Empty;

        [ValidateNever]
        public List<SelectListItem>? Roles { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }
        [ValidateNever]
        public List<SelectListItem>? Departments { get; set; }
    }
}
