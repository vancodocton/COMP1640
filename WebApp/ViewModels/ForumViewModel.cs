using WebApp.Models;

namespace WebApp.ViewModels
{
    public class ForumViewModel
    {
        public ForumViewModel(List<Idea> ideas, List<Category> categories)
        {
            Ideas = ideas;
            Categories = categories;
        }

        public List<Idea> Ideas { get; set; }

        public int? CategoryId { get; set; }
        public List<Category> Categories { get; set; }
    }
}
