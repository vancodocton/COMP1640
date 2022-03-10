namespace WebApp.Hubs
{
    public partial class IdeaInteractHub
    {
        public class IdeaReactReponse
        {
            public int? IdeaId { get; set; }

            public string? React { get; set; }
        }
    }
}
