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
            var user = await userManager.GetUserAsync(User);
            // Get a list of users in the role
            var usersWithPermission = userManager.GetUsersInRoleAsync(Role.Staff).Result;
            // Then get a list of the ids of these users
            var idsWithPermission = usersWithPermission.Select(u => u.Id);
            var model = await userManager.Users
                .Where(u => idsWithPermission.Contains(u.Id))
                .Where(u => u.DepartmentId == user.DepartmentId)
                .Select(u => new StatisticalAnalysisViewModel()
                {
                    ideaCount = u.Ideas.Count,
                    reactCount = u.Reacts.Count,
                    commentCount = u.Comments.Count,
                    Id = u.Id,
                    Email = u.Email,
                })
                .OrderByDescending(u => u.ideaCount)
                .ThenByDescending(u => u.reactCount)
                .ToListAsync();         
            return View(model);
        }
    }
}