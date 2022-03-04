using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Hubs
{
    public class IdeaReactHub : Hub
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<React> logger;


        public IdeaReactHub(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ILogger<React> logger)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task<Task> ThumbUp(int ideaId)
        {
            var idea = await dbContext.Idea.FirstOrDefaultAsync(i => i.Id == ideaId);

            var user = await userManager.GetUserAsync(Context.User);

            var react = await dbContext.React.FirstOrDefaultAsync(r => r.UserId == user.Id && r.IdeaId == ideaId);

            if (react == null)
            {
                react = new React()
                {
                    IdeaId = ideaId,
                    UserId = user.Id,
                    Type = ReactType.ThumbUp
                };

                idea.ThumbUp++;

                dbContext.React.Add(react);
            }
            else
            {
                idea.ThumbUp++;
                idea.ThumbDown--;

                react.Type = ReactType.ThumbUp;
                dbContext.React.Attach(react);
            }

            dbContext.Idea.Attach(idea);

            var row = await dbContext.SaveChangesAsync();

            return Clients.All.SendAsync("ThumbUp", "Thumbed up idea! " + ideaId + " " + user.Id);
        }

        public async Task<Task> ThumbDown(int ideaId)
        {
            var idea = await dbContext.Idea.FirstOrDefaultAsync(i => i.Id == ideaId);

            var user = await userManager.GetUserAsync(Context.User);

            var react = await dbContext.React.FirstOrDefaultAsync(r => r.UserId == user.Id && r.IdeaId == ideaId);

            if (react == null)
            {
                react = new React()
                {
                    IdeaId = ideaId,
                    UserId = user.Id,
                    Type = ReactType.ThumbDown
                };

                idea.ThumbDown++;

                dbContext.React.Add(react);
            }
            else
            {
                idea.ThumbDown++;
                idea.ThumbUp--;

                react.Type = ReactType.ThumbDown;
                dbContext.React.Attach(react);
            }

            dbContext.Idea.Attach(idea);

            var row = await dbContext.SaveChangesAsync();

            return Clients.All.SendAsync("ThumbDown", "Thumbed down idea! " + ideaId + " " + user.Id);
        }

        public async Task<Task> RemoveReact(int ideaId)
        {
            var idea = await dbContext.Idea.FirstOrDefaultAsync(i => i.Id == ideaId);

            var user = await userManager.GetUserAsync(Context.User);

            var react = await dbContext.React.FirstOrDefaultAsync(r => r.UserId == user.Id && r.IdeaId == ideaId);

            if (react == null)
            {
                //react = new React()
                //{
                //    IdeaId = ideaId,
                //    UserId = user.Id,
                //    Type = ReactType.ThumbDown
                //};
                //_ = await dbContext.React.AddAsync(react);
            }
            else
            {
                if (react.Type == ReactType.ThumbUp)
                    idea.ThumbUp--;
                else
                    idea.ThumbDown--;


                dbContext.React.Remove(react);
            }

            dbContext.Idea.Attach(idea);

            var row = await dbContext.SaveChangesAsync();

            return Clients.All.SendAsync("RemoveReact", "Remove react  idea! " + ideaId + " " + user.Id);
        }
    }
}
