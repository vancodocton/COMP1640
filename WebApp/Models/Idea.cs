using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Idea
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        public ApplicationUser? User { get; set; }

        public bool IsIncognito { get; set; } = false;

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Content { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<React> Reacts { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = null!;

        public int ThumbUp { get; set; } = 0;

        public int ThumbDown { get; set; } = 0;

        public int NumComment { get; set; } = 0;

        public int NumView { get; set; } = 0;
    }
}
