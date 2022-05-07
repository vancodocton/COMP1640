using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Web.Models
{
    public class React
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int IdeaId { get; set; }
        public Idea Idea { get; set; } = null!;

        public ReactType Type { get; set; }
    }

    public enum ReactType
    {
        ThumbUp,
        ThumbDown,
        Null
    }
}
