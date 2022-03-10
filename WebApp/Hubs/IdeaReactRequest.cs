namespace WebApp.Hubs
{
    public partial class IdeaInteractHub
    {
        public class IdeaReactRequest
        {
            public int? IdeaId { get; set; }

            public string? Type { get; set; }

            public string? NewReact { get; set; }
        }
    }
}
