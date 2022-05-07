using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using App.Core.Entities;

namespace App.Web.ViewModels
{
    [BindProperties]
    public class ForumSearch
    {
        [BindProperty(Name = "order", SupportsGet = true)]
        public string? Order { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Invalid Page number")]
        public int Page { get; set; } = 1;

        [BindProperty(Name = "cid", SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(Name = "dpmtId", SupportsGet = true)]
        public int? DepartmentId { get; set; }

        [ValidateNever]
        public List<SelectListItem> Departments { get; set; } = new()
        {
            new SelectListItem("All departments", "All"),
        };

        [ValidateNever]
        public ICollection<Category> Categories { get; set; } = null!;
    }
}
