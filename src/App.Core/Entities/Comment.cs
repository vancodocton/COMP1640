using System.ComponentModel.DataAnnotations;

namespace App.Core.Entities
{
    public class Comment  : BaseEntity<int>
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public int IdeaId { get; set; }
        public Idea Idea { get; set; } = null!;
        
        [StringLength(200)]
        public string Content { get; set; } = null!;
    }
}
