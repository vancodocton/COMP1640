using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using WebApp.Data;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Authorize]
    public class IdeaController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender sender;

        public IdeaController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender sender
            )
        {
            this.context = context;
            this.userManager = userManager;
            this.sender = sender;
        }

        [Authorize(Roles = Role.Staff)]
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            var model = new CreateIdeaViewModel()
            {
                Categories = await context
                .Category.Select(c => new SelectListItem()
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync(),
            };

            return View(model);
        }

        [Authorize(Roles = Role.Staff)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateIdeaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(HttpContext.User);

                var idea = new Idea()
                {
                    Title = model.Title,
                    Content = model.Content,
                    CategoryId = model.CategoryId,
                    UserId = user.Id,
                    IsIncognito = model.IsIncognito,
                };

                await context.Idea.AddAsync(idea);
                var count = await context.SaveChangesAsync();

                var coors = (await userManager.GetUsersInRoleAsync(Role.Coordinator))
                    .Where(c => c.DepartmentId == user.DepartmentId)
                    .ToList();

                var url = Url.ActionLink("Idea", "Forum", new { id = idea.Id });

                foreach (var coor in coors)
                {
                    await sender.SendEmailAsync(coor.Email, "New Idea is created by your department staff", $"<a href={HtmlEncoder.Default.Encode(url)}>Idea</a>");

                }

                return RedirectToAction(nameof(Index), new {id = idea.Id});
            }

            model.Categories = await context
                .Category.Select(c => new SelectListItem()
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            return View(model);
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (id == null) return BadRequest();

            var model = await context.Idea
                .Include(i => i.Comments).ThenInclude(c => c.User)
                .Include(i => i.Category)
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (model == null) return BadRequest();

            return View(model);
        }
    }
}
