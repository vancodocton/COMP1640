using App.Web.Data;
using App.Core.Entities;

namespace App.Web.ViewModels
{
    public class ForumViewModel
    {
        public PaginatedList<Idea> Ideas { get; set; } = null!;

        public ApplicationUser User { get; set; } = null!;

        public Category? Category { get; set; }

        public ForumSearch Search { get; set; } = new();
    }
}
