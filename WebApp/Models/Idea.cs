using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Idea
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<FileOnFileSystem> FileOnFileSystems { get; set; } = null!;

        public bool IsIncognito { get; set; } = false;

        public string Title { get; set; } = null!;

        [DataType(DataType.MultilineText)]
        public string Content { get; set; } = null!;

        public ICollection<React> Reacts { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = null!;

        public int ThumbUp { get; set; } = 0;

        public int ThumbDown { get; set; } = 0;

        public int NumComment { get; set; } = 0;

        public int NumView { get; set; } = 0;
    }
}
