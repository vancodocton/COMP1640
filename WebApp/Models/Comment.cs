using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int IdeaId { get; set; }
        public Idea Idea { get; set; } = null!;

        public string Content { get; set; } = null!;
    }
}
