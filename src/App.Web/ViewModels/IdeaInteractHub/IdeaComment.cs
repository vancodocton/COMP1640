namespace App.Web.ViewModels.IdeaInteractHub
{
    public class IdeaComment
    {
        public int? CommentId { get; set; }

        public int? IdeaId { get; set; }

        public string? UserName { get; set; }

        public string? Content { get; set; }

        public int? DepartmentId { get; set; }
    }
}