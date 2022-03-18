using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using WebApp.Models.Requests;
using WebApp.Models.Responses;

namespace WebApp.Hubs
{
    [Authorize]
    public class IdeaInteractHub : Hub
    {

        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<IdeaInteractHub> logger;

        public IdeaInteractHub(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            ILogger<IdeaInteractHub> logger)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task RegisterIdeaStatus(int ideaId)
        {
            var idea = await dbContext.Idea.FirstAsync(i => i.Id == ideaId);
            // Register User to group following idea status
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{idea.Id}");

            var user = await userManager.GetUserAsync(Context.User);
            var react = await dbContext.React
                .FirstOrDefaultAsync(r => r.UserId == user.Id && r.IdeaId == ideaId);

            var response = new IdeaReactReponse()
            {
                IdeaId = ideaId,
            };

            if (react != null)
            {
                response.React = react.Type.ToString();
            }
            else
            {
                // add react with null type to count num view
                dbContext.React.Add(new React()
                {
                    UserId = user.Id,
                    IdeaId = ideaId,
                    Type = ReactType.Null
                });

                idea.NumView++;
                dbContext.Idea.Attach(idea);

                await dbContext.SaveChangesAsync();
            }

            if (Context.User.IsInRole(Role.Staff))
                await ReponseIdeaStatus(idea);
            else
                await ReponseIdeaStatus(idea, isCommented: false, isReacted: false);

            await Clients.User(user.Id).SendAsync("ResponseUserIdeaReaction", response);
        }

        private async Task ReponseIdeaStatus(Idea idea, bool? isCommented = null, bool? isReacted = null)
        {
            idea.Category = await dbContext.Category.FirstAsync(c => c.Id == idea.CategoryId);

            var ideaStatus = new IdeaStatus()
            {
                IdeaId = idea.Id,
                ThumbUp = idea.ThumbUp,
                ThumbDown = idea.ThumbDown,
                NumComment = idea.NumComment,
                NumView = idea.NumView,
                IsCommented = idea.Category.FinalDueDate == null || DateTime.Now <= idea.Category.FinalDueDate,
                IsReacted = true
            };

            if (isCommented != null)
                ideaStatus.IsCommented = isCommented.Value;

            if (isReacted != null)
                ideaStatus.IsReacted = isReacted.Value;

            await Clients.Groups($"{idea.Id}").SendAsync("IdeaStatus", ideaStatus);
        }

        [Authorize(Roles = Role.Staff)]
        public async Task ReactIdea(IdeaReactRequest request)
        {
            if (request.Type == null || request.IdeaId == null)
                throw new HubException("Request type cannot be null");

            var idea = await dbContext.Idea.FirstAsync(i => i.Id == request.IdeaId);

            var user = await userManager.GetUserAsync(Context.User);

            var react = await dbContext.React
                .FirstOrDefaultAsync(r => r.UserId == user.Id && r.IdeaId == request.IdeaId);

            try
            {
                switch (request.Type.ToLower())
                {
                    case "create":
                        if (react == null)
                            await AddReact(idea, request);
                        else
                            await UpdateReact(react, idea, request);
                        break;
                    case "update":
                        if (react == null)
                            await AddReact(idea, request);
                        else
                            await UpdateReact(react, idea, request);
                        break;
                    case "delete":
                        if (react != null)
                            await RemoveReact(react, idea, request);
                        break;
                    default:
                        throw new ArgumentException("Invalid reqest type");
                }
            }
            catch (Exception ex)
            {
                throw new HubException("", ex);
            }

            //var row = await dbContext.SaveChangesAsync();
            var response = new IdeaReactReponse()
            {
                IdeaId = request.IdeaId,
                React = request.NewReact
            };

            await ReponseIdeaStatus(idea);
            await Clients.User(user.Id).SendAsync("ResponseUserIdeaReaction", response);
        }

        private async Task AddReact(Idea idea, IdeaReactRequest request)
        {
            if (request.NewReact == null)
                throw new ArgumentNullException(nameof(request.NewReact));

            ApplicationUser? user = await userManager.GetUserAsync(Context.User);

            var newReact = new React()
            {
                IdeaId = idea.Id,
                UserId = user.Id,
                Type = (ReactType)Enum.Parse(typeof(ReactType), request.NewReact)
            };

            idea = CountNewReact(idea, newReact.Type);

            dbContext.React.Add(newReact);
            dbContext.Idea.Append(idea);
            await dbContext.SaveChangesAsync();
        }

        private async Task UpdateReact(React react, Idea idea, IdeaReactRequest request)
        {
            if (request.NewReact == null)
                throw new ArgumentNullException(nameof(request.NewReact));

            var oldReactType = react.Type;
            var newReactType = (ReactType)Enum.Parse(typeof(ReactType), request.NewReact);

            react.Type = newReactType;

            idea = CountNewReact(idea, newReactType);
            idea = CountOldReact(idea, oldReactType);

            dbContext.React.Append(react);
            dbContext.Idea.Append(idea);
            await dbContext.SaveChangesAsync();
        }

        private async Task RemoveReact(React react, Idea idea, IdeaReactRequest request)
        {
            idea = CountOldReact(idea, react.Type);

            react.Type = ReactType.Null;

            dbContext.React.Attach(react);
            dbContext.Idea.Attach(idea);
            await dbContext.SaveChangesAsync();
        }

        private Idea CountNewReact(Idea idea, ReactType reactType)
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

        private Idea CountOldReact(Idea idea, ReactType reactType)
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
