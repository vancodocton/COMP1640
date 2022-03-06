﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Hubs
{
    [Authorize]
    public partial class IdeaReactHub : Hub
    {

        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<IdeaReactHub> logger;

        public IdeaReactHub(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ILogger<IdeaReactHub> logger)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task ReactIdea(IdeaReactRequest request)
        {
            if (request.Type == null || request.IdeaId == null)
                throw new HubException("Request type cannot be null");

            Idea idea = await dbContext.Idea.FirstAsync(i => i.Id == request.IdeaId);

            ApplicationUser? user = await userManager.GetUserAsync(Context.User);

            React? react = await dbContext.React
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

            await Clients.User(user.Id).SendAsync("ReactIdeaResponse", response);
            await PushIdeaReactStatus(idea.Id);
        }

        public async Task RegisterIdeaReactStatus(int ideaId)
        {
            var idea = await dbContext.Idea.FirstAsync(i => i.Id == ideaId);

            var user = await userManager.GetUserAsync(Context.User);

            var react = await dbContext.React
                .FirstOrDefaultAsync(r => r.UserId == user.Id && r.IdeaId == ideaId);

            await Groups.AddToGroupAsync(Context.ConnectionId, ideaId.ToString());

            var response = new IdeaReactReponse()
            {
                IdeaId = ideaId,
            };

            if (react != null)
            {
                response.React = react.Type.ToString();
            }

            await Clients.User(user.Id).SendAsync("ReactIdeaResponse", response);
            await PushIdeaReactStatus(ideaId);
        }

        private async Task PushIdeaReactStatus(int ideaId)
        {
            var idea = await dbContext.Idea.FirstAsync(i => i.Id == ideaId);

            var response = new IdeaReactStatus()
            {
                IdeaId = idea.Id,
                ThumbUp = idea.ThumbUp,
                ThumbDown = idea.ThumbDown
            };

            await Clients.Groups(ideaId.ToString()).SendAsync("IdeaReactStatus", response);
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

            dbContext.React.Remove(react);
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
