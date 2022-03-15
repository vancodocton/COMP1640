using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        // Change UserId is not required to save the comment after deleted user
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int IdeaId { get; set; }
        public Idea? Idea { get; set; }

        public string Content { get; set; } = null!;
    }
}
