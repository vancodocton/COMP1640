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
using WebApp.Models.Hubs;

namespace WebApp.Areas.Forum.Controllers
{
    [ApiController]
    [Route("Forum/{controller}/{action}/{id?}")]
    [Area("Forum")]
    [Authorize]
    public class CommentController : ControllerBase
    {

        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender sender;
        private readonly IHubContext<IdeaInteractHub, IIdeaInteractClient> hubContext;

        public CommentController(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IEmailSender sender,
            IHubContext<IdeaInteractHub, IIdeaInteractClient> hubContext
            )
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.sender = sender;
            this.hubContext = hubContext;
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Comment request)
        {
            if (!User.IsInRole(Role.Staff))
                return StatusCode(StatusCodes.Status401Unauthorized, "Only Staff can comment idea.");

            if (request.IdeaId == 0)
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
                .SingleOrDefaultAsync(i => i.Id == request.IdeaId);

            if (idea == null || idea.Category == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Idea is deleted or not existed.");
            }

            if (DateTime.Now > idea.Category.FinalDueDate)
                return StatusCode(StatusCodes.Status400BadRequest, "Cannot comment idea becaufe its final due date is over.");

            var comment = new Comment()
            {
                IdeaId = request.IdeaId,
                UserId = user.Id,
                Content = request.Content,
            };

            await dbContext.Comment.AddAsync(comment);
            idea.NumComment++;

            var row = await dbContext.SaveChangesAsync();

            if (user.Id != idea.UserId)
            {
                var ideaUrl = Url.ActionLink("Index", "Idea", new { id = idea.Id });

                if (ideaUrl != null && idea.User!.EmailConfirmed)
                {
                    var htmlMessage = $"{user.Email} commented about your idea '{idea.Title}'. Please click <a href={HtmlEncoder.Default.Encode(ideaUrl)}>at here</a> to see new comment.";
                    await sender.SendEmailAsync(idea.User.Email, "New comment about your idea", htmlMessage);
                }
            }

            var response = new IdeaComment()
            {
                IdeaId = idea.Id,
                UserName = user.UserName,
                CommentId = comment.Id,
                Content = comment.Content
            };

            await hubContext.Clients.Groups($"{idea.Id}").ReceiveComment(response);

            var responseStatus = new IdeaIntereactionStatus()
            {
                IdeaId = idea.Id,
                ThumbUp = idea.ThumbUp,
                ThumbDown = idea.ThumbDown,
                NumView = idea.NumView,
                NumComment = idea.NumComment
            };

            await hubContext.Clients.Groups($"{idea.Id}").ReceiveInteractionStatus(responseStatus);

            return Ok(comment.Id);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Api worked.");
        }

        [HttpDelete]
        [Authorize(Roles = $"{Role.Staff},{Role.Coordinator}")]

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return StatusCode(StatusCodes.Status400BadRequest);

            var cmt = await dbContext.Comment
                .Include(c => c.Idea)
                .SingleOrDefaultAsync(i => i.Id == id);

            if (cmt == null)
                return StatusCode(StatusCodes.Status404NotFound);

            var user = await userManager.GetUserAsync(User);

            if (User.IsInRole(Role.Staff))
            {
                if (cmt.UserId == user.Id)
                {
                    cmt.Idea.NumComment--;

                    var numCmt = cmt.Idea.NumComment;

                    dbContext.Comment.Remove(cmt);

                    await dbContext.SaveChangesAsync();

                    return Ok(numCmt);
                }
                else
                    return StatusCode(StatusCodes.Status401Unauthorized, "Can Delete comment of other Staff");
            }

            else if (User.IsInRole(Role.Coordinator))
            {
                var department = await dbContext.Department
                    .SingleAsync(d => d.Id == user.DepartmentId);

                cmt.User = await userManager.FindByIdAsync(cmt.UserId);

                if (cmt.User.DepartmentId == department.Id)
                {
                    cmt.Idea.NumComment--;

                    var numCmt = cmt.Idea.NumComment;

                    dbContext.Comment.Remove(cmt);

                    await dbContext.SaveChangesAsync();

                    return Ok(numCmt);
                }
                else
                    return StatusCode(StatusCodes.Status401Unauthorized, "Can Delete comment of Staff in other department");


            }
            else
                return StatusCode(StatusCodes.Status401Unauthorized, "Only Staff or Coordinator can Delete comment");

        }
    }
}
