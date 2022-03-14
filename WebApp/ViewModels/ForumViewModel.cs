using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public class ForumViewModel
    {
        public PaginatedList<Idea> Ideas { get; set; } = null!;

        public string Sort { get; set; } = "lastest";

        [Range(1, int.MaxValue, ErrorMessage = "Invalid Page number")]
        public int Page { get; set; } = 1;
        
        public int? CategoryId { get; set; }

        [BindNever]
        public ICollection<Category> Categories { get; set; } = null!;
    }
}
