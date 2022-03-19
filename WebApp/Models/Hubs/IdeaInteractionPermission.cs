namespace WebApp.Models.Hubs
{
    public class IdeaInteractionPermission
    {
        public int? IdeaId { get; set; }

        public bool? IsCommented { get; set; } = false;

        public bool? IsReacted { get; set; } = false;
    }
}
