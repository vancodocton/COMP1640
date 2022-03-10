namespace WebApp.Hubs
{
    public partial class IdeaInteractHub
    {
        public class IdeaStatus
        {
            public int? IdeaId { get; set; }

            public int? ThumbUp { get; set; }

            public int? ThumbDown { get; set; }

            public int? NumComment { get; set; }

            public int? NumView { get; set; }
        }
    }
}
