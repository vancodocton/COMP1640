using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Areas.Coordinator.Controllers
{
	[Area("Coordinator")]
	[Authorize(Roles = Role.Coordinator)]
	public class IdeaController : Controller
	{
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender sender;

        public IdeaController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager
            )
        {
            this.context = context;
            this.userManager = userManager;
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
