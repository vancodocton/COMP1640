using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Authorize(Roles = $"{Role.Staff},{Role.Coordinator},{Role.Manager}")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender sender;
        private readonly int pageSize = 5;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender sender
            )
        {
            this.context = context;
            this.userManager = userManager;
            this.sender = sender;
        }

        public async Task<ActionResult> Index(int? cid, string? sort, int page = 1)
        {
            var ideas = context.Idea
                .Include(x => x.Category)
                .Include(x => x.User)
                .Include(x => x.Comments
                    .OrderByDescending(i => i.Id)
                    .Take(2)
                    .OrderBy(i => i.Id))
                .AsQueryable();

            if (cid != null)
            {
                ideas = ideas.Where(i => i.CategoryId == cid);
                ViewData["cid"] = cid;
            }

            switch (sort)
            {
                case null:
                case "":
                case "lastest":
                default:
                    ideas = ideas.OrderByDescending(i => i.Id);
                    break;
                case "popular":
                    ideas = ideas
                        .OrderByDescending(i => i.ThumbUp - i.ThumbDown)
                        .ThenByDescending(i => i.ThumbUp)
                        .ThenByDescending(i => i.Id);
                    break;
                case "topview":
                    ideas = ideas
                        .OrderByDescending(i => i.NumView)
                        .ThenByDescending(i => i.Id);
                    break;
            }

            var model = new ForumViewModel();

            model.Ideas = await PaginatedList<Idea>.CreateAsync(ideas, page, pageSize);

            model.Categories = await context.Category.ToListAsync();
            model.Page = page;
            model.Sort = sort ?? "";
            model.CategoryId = cid;

            var user = await userManager.GetUserAsync(User);
            var department = await context.Department
                .SingleOrDefaultAsync(d => d.Id == user.DepartmentId);
            ViewData["UserDepartmentId"] = department?.Id  ;

            return View(model);
        }
    }
}
