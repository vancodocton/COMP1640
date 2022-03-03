using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Data;
using WebApp.Models;
using WebApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize(Roles = Role.Staff + "," + Role.Coordinator)]
    public class ForumController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public ForumController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager
            )
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<ActionResult> Index(int? cid, string? order)
        {
            var categories = await context.Category.ToListAsync();

            var ideas = context.Idea
                .Include(x => x.Category)
                .Include(x => x.User)
                .AsQueryable();

            if (cid != null)
            {
                ideas = ideas.Where(i => i.CategoryId == cid);
                ViewData["cid"] = cid;
            }

            switch (order)
            {
                case null:
                case "lastest":
                    ideas = ideas.OrderByDescending(i => i.Id);
                    break;
                case "popular":
                    break;
                case "topview":
                    break;
                default:
                    return BadRequest("Invalid order option");
            }


            var model = new ForumViewModel(await ideas.ToListAsync(), categories);

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            var model = new IdeaViewModel()
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IdeaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = (await userManager.GetUserAsync(HttpContext.User)).Id;
                var idea = new Idea()
                {
                    Title = model.Title,
                    Content = model.Content,
                    CategoryId = model.CategoryId,
                    UserId = userId,
                };

                await context.Idea.AddAsync(idea);
                var count = await context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
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

        public async Task<IActionResult> Idea(int? id)
        {
            if (id == null) return BadRequest();

            var model = await context.Idea
                .Include(i => i.Category)
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (model == null) return BadRequest();            

            return View(model);
        }
    }
}
