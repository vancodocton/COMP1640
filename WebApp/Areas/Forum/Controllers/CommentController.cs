using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using WebApp.Data;
using WebApp.Hubs;
using WebApp.Models;

namespace WebApp.Areas.Forum.Controllers
{
    [ApiController]
    [Route("Forum/[controller]/[action]")]
    [Area("Forum")]
    [Authorize]
    public class CommentController : ControllerBase
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
            if (!User.IsInRole(Role.Staff))
                return StatusCode(StatusCodes.Status401Unauthorized, "Only Staff can comment idea.");

            if (request.IdeaId == null)
                return StatusCode(StatusCodes.Status400BadRequest, "IdeaId cannot be null.");

            if (string.IsNullOrWhiteSpace(request.Content))
                return StatusCode(StatusCodes.Status400BadRequest, "Comment cannot be null or empty.");

            var user = await userManager.GetUserAsync(User);

            var idea = await dbContext.Idea
                .Include(i => i.User)
                .Include(i => i.Category)
                //.Select(i => new Idea()
                //{
                //    Id = i.Id,
                //    NumComment = i.NumComment,
                //    CategoryId = i.CategoryId,
                //    Category = new Category()
                //    {
                //        Id = i.CategoryId,
                //        DueDate = i.Category!.DueDate,
                //        FinalDueDate = i.Category!.FinalDueDate,
                //    },
                //    ThumbUp = i.ThumbUp,
                //    ThumbDown = i.ThumbDown,
                //    NumView = i.NumView,
                //    User = new ApplicationUser()
                //    {
                //        EmailConfirmed = i.User!.EmailConfirmed,
                //        Email = i.User.Email
                //    },
                //})
                .FirstOrDefaultAsync(i => i.Id == request.IdeaId);

            if (idea == null || idea.Category == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Idea is deleted or not existed.");
            }

            if (DateTime.Now > idea.Category.FinalDueDate)
                return StatusCode(StatusCodes.Status400BadRequest, "Cannot comment idea becaufe its final due date is over.");

            var comment = new Comment()
            {
                IdeaId = request.IdeaId.Value,
                UserId = user.Id,
                Content = request.Content,
            };

            await dbContext.Comment.AddAsync(comment);
            idea.NumComment++;

            var row = await dbContext.SaveChangesAsync();

            if (user.Id != idea.UserId)
            {
                var ideaUrl = Url.ActionLink("Idea", "Forum", new { id = idea.Id });

                if (ideaUrl != null && idea.User!.EmailConfirmed)
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
                NumView = idea.NumView,
                NumComment = idea.NumComment,
                IsCommented = idea.Category.FinalDueDate == null || DateTime.Now <= idea.Category.FinalDueDate,
                IsReacted = true
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
