namespace WebApp.Hubs
{
    public partial class IdeaInteractHub
    {
        public class IdeaReactStatus
        {
            public int? IdeaId { get; set; }

            public int? ThumbUp { get; set; }

            public int? ThumbDown { get; set; }
        }
    }
}
