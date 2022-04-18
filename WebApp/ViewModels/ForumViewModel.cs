using WebApp.Data;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public class ForumViewModel
    {
        public PaginatedList<Idea> Ideas { get; set; } = null!;

        public ApplicationUser User { get; set; } = null!;

        public Category? Category { get; set; }

        public ForumSearch Search { get; set; } = new();
    }
}
