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
    public class StatisticalController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender sender;

        public StatisticalController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender sender
            )
        {
            this.context = context;
            this.userManager = userManager;
            this.sender = sender;
        }
        public async Task<ActionResult> Index()
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            var idea = await context.Idea.FirstOrDefaultAsync(i => i.Id == 1);
            var coors = user.DepartmentId;
            var model = await userManager.Users.Where(u => u.DepartmentId == coors)
                .Select(u => new StatisticalAnalysisViewModel()
                {
                    Id = u.Id,
                    Email = u.Email,
                })
                .ToListAsync();
            return View(model);
        }
    }
}