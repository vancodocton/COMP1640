using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class React
    {
        public int Id { get; set; }

        // Change UserId is not required to save the reaction after deleted user
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int IdeaId { get; set; }
        public Idea? Idea { get; set; }

        [Required]
        public ReactType Type { get; set; }
    }

    public enum ReactType
    {
        ThumbUp,
        ThumbDown,
        Viewed
    }
}
