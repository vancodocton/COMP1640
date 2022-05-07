namespace App.Core.Entities
{
    public class React : BaseEntity<int>
    {
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int IdeaId { get; set; }
        public Idea Idea { get; set; } = null!;

        public ReactType Type { get; set; }
    }
}
