using System.ComponentModel.DataAnnotations;

namespace App.Core.Entities
{
    public class Idea : BaseEntity<int>
    {
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public bool IsIncognito { get; set; } = false;

        [StringLength(100)]
        public string Title { get; set; } = null!;

        [DataType(DataType.MultilineText), StringLength(10000)]
        public string Content { get; set; } = null!;

        public int ThumbUp { get; set; } = 0;

        public int ThumbDown { get; set; } = 0;

        public int NumComment { get; set; } = 0;

        public int NumView { get; set; } = 0;

        public virtual ICollection<FileOnFileSystem> FileOnFileSystems { get; set; } = null!;

        public virtual ICollection<React> Reacts { get; set; } = null!;

        public virtual ICollection<Comment> Comments { get; set; } = null!;
    }
}
