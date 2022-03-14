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
using WebApp.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApp.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Authorize(Roles = Role.Staff + "," + Role.Coordinator)]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender sender;

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
    }
}
