namespace App.Core.Entities
{
    public class FileOnFileSystem : BaseEntity<int>
    {
        public int IdeaId { get; set; }

        public Idea Idea { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string FileType { get; set; } = null!;

        public string Extension { get; set; } = null!;

        public DateTime UploadTime { get; set; } = DateTime.UtcNow;

        public string FilePath { get; set; } = null!;
    }
}
