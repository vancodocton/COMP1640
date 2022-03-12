﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using WebApp.Data;
using WebApp.Hubs;
using WebApp.Models;

namespace WebApp.Controllers
{
    //[ApiController]
    //[Route("[controller]")]
    [Authorize]
    public class CommentController : Controller
    {

        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender sender;
        private readonly IHubContext<IdeaInteractHub> hubContext;

        public CommentController(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IEmailSender sender,
            IHubContext<IdeaInteractHub> hubContext
            )
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.sender = sender;
            this.hubContext = hubContext;
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromBody] IdeaCommentRequest request)
        {
            if (request.IdeaId == null || string.IsNullOrWhiteSpace(request.Content))
                return BadRequest();

            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return BadRequest();

            var idea = await dbContext.Idea
                .Include(i => i.User)
                .FirstAsync(i => i.Id == request.IdeaId);

            var comment = new Comment()
            {
                IdeaId = request.IdeaId,
                UserId = user.Id,
                Content = request.Content,
            };

            await dbContext.Comment.AddAsync(comment);
            idea.NumComment++;
            dbContext.Idea.Attach(idea);

            var row = await dbContext.SaveChangesAsync();

            if (user.Id != idea.UserId)
            {
                var ideaUrl = Url.ActionLink("Idea", "Forum", new { id = idea.Id });

                if (ideaUrl != null)
                {
                    var htmlMessage = $"{user.Email} commented about your idea '{idea.Title}'. Please click <a href={HtmlEncoder.Default.Encode(ideaUrl)}>at here</a> to see new comment.";
                    await sender.SendEmailAsync(idea.User.Email, "New comment about your idea", htmlMessage);
                }
            }

            var responseComment = new IdeaCommentResponse()
            {
                IdeaId = idea.Id,
                UserName = user.UserName,
                CommentId = comment.Id,
                Content = comment.Content
            };

            await hubContext.Clients.Group($"{idea.Id}").SendAsync("ReceiveComment", responseComment);

            var responseStatus = new IdeaStatus()
            {
                IdeaId = idea.Id,
                ThumbUp = idea.ThumbUp,
                ThumbDown = idea.ThumbDown,
                NumComment = idea.NumComment
            };

            await hubContext.Clients.Groups($"{idea.Id}").SendAsync("IdeaStatus", responseStatus);

            return Ok(idea.Id);
        }

        public IActionResult Index()
        {
            return Ok();
        }
    }
}
