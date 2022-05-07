using App.Web.ViewModels.IdeaInteractHub;

namespace App.Web.Services.Hubs
{
    public interface IIdeaInteractClient
    {
        Task ReceiveRegisteredConfirmation(IdeaInteractionPermission permission, IdeaReaction react);

        Task ReceiveInteractionStatus(IdeaIntereactionStatus status);

        Task ReceiveComment(IdeaComment comment);

        Task RevokeSentComment(RevokeSentIdeaResponse response);

        Task ReceiveReaction(IdeaReaction reaction);
    }
}
