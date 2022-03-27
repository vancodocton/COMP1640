using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using WebApp.Models.Hubs;

namespace WebApp.Hubs
{
    public interface IIdeaInteractClient
    {
        Task ReceiveRegisteredConfirmation(IdeaInteractionPermission permission, IdeaReaction react);

        Task ReceiveInteractionStatus(IdeaIntereactionStatus status);

        Task ReceiveComment(IdeaComment comment);

        Task RevokeSentComment(RevokeSentIdeaResponse response);

        Task ReceiveReaction(IdeaReaction reaction);
    }

    [Authorize]
    public class IdeaInteractHub : Hub<IIdeaInteractClient>
    {

        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<IdeaInteractHub> logger;

        private async Task<bool> IsInRoleStaffAsync(ApplicationUser user) => await userManager.IsInRoleAsync(user, Role.Staff);

        public IdeaInteractHub(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            ILogger<IdeaInteractHub> logger)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            var ideaId = int.Parse(httpContext!.Request.Query["IdeaId"]);

            logger.LogInformation(ideaId.ToString());

            var idea = await dbContext.Idea.FirstAsync(i => i.Id == ideaId);
            var user = await userManager.GetUserAsync(Context.User);

            var permission = await GetUserReactionPemissionAsync(user, idea);

            var reaction = await GetUserReactionAsync(user, idea);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"{idea.Id}");

            await Clients.Caller.ReceiveRegisteredConfirmation(permission, reaction);

            await ReponseIdeaStatusToGroup(idea);
        }

        private async Task<IdeaReaction> GetUserReactionAsync(ApplicationUser user, Idea idea)
        {
            var res = new IdeaReaction() { IdeaId = idea.Id, UserName = user.UserName };

            var react = await dbContext.React
                .FirstOrDefaultAsync(r => r.UserId == user.Id && r.IdeaId == idea.Id);

            if (react == null)
            {
                react = new React()
                {
                    UserId = user.Id,
                    IdeaId = idea.Id,
                    Type = ReactType.Null
                };

                dbContext.React.Add(react);
                idea.NumView++;
                dbContext.Idea.Attach(idea);

                await dbContext.SaveChangesAsync();
            }
            else
                res.ReactType = react.Type.ToString();

            return res;
        }

        private async Task<IdeaInteractionPermission> GetUserReactionPemissionAsync(ApplicationUser user, Idea idea)
        {
            var permission = new IdeaInteractionPermission();

            if (await IsInRoleStaffAsync(user))
            {
                idea.Category = await dbContext.Category.FirstAsync(c => c.Id == idea.CategoryId);

                permission.IdeaId = idea.Id;
                permission.IsCommented = idea.Category.FinalDueDate == null || DateTime.UtcNow <= idea.Category.FinalDueDate;
                permission.IsReacted = idea.Category.FinalDueDate == null || DateTime.UtcNow <= idea.Category.FinalDueDate;
            }

            return permission;
        }

        private async Task ReponseIdeaStatusToGroup(Idea idea)
        {
            var ideaStatus = new IdeaIntereactionStatus()
            {
                IdeaId = idea.Id,
                ThumbUp = idea.ThumbUp,
                ThumbDown = idea.ThumbDown,
                NumComment = idea.NumComment,
                NumView = idea.NumView
            };

            await Clients.Groups($"{idea.Id}").ReceiveInteractionStatus(ideaStatus);
        }

        [Authorize(Roles = Role.Staff)]
        public async Task ReactIdea(IdeaReaction reaction)
        {
            if (reaction.IdeaId == null)
                throw new HubException("Request type cannot be null");

            var idea = await dbContext.Idea
                .Include(i => i.Category)
                .FirstAsync(i => i.Id == reaction.IdeaId);

            if (idea.Category.FinalDueDate == null || DateTime.UtcNow <= idea.Category.FinalDueDate)
            {
                var user = await userManager.GetUserAsync(Context.User);

                var react = await dbContext.React
                    .FirstAsync(r => r.UserId == Context.UserIdentifier && r.IdeaId == reaction.IdeaId);

                var oldReactType = react.Type;
                var newReactType = (ReactType)Enum.Parse(typeof(ReactType), reaction.ReactType ?? "Null");

                react.Type = newReactType;

                idea = CountNewReact(idea, newReactType);
                idea = CountOldReact(idea, oldReactType);

                dbContext.React.Append(react);
                dbContext.Idea.Append(idea);
                await dbContext.SaveChangesAsync();

                reaction.UserName = Context.User.Identity.Name;

                await Clients.User(Context.UserIdentifier!).ReceiveReaction(reaction);

                await ReponseIdeaStatusToGroup(idea);
            }
            else
            {
                throw new HubException("Cannot react of idea in category over its final due date.");
            }
        }

        private static Idea CountNewReact(Idea idea, ReactType reactType)
        {
            switch (reactType)
            {
                case ReactType.ThumbUp:
                    idea.ThumbUp++;
                    break;
                case ReactType.ThumbDown:
                    idea.ThumbDown++;
                    break;
            }
            return idea;
        }

        private static Idea CountOldReact(Idea idea, ReactType reactType)
        {
            switch (reactType)
            {
                case ReactType.ThumbUp:
                    idea.ThumbUp--;
                    break;
                case ReactType.ThumbDown:
                    idea.ThumbDown--;
                    break;
            }
            return idea;
        }
    }
}
