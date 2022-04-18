using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Authorize(Roles = $"{Role.Coordinator},{Role.Manager}")]
    public class AnalysisController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public AnalysisController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager
            )
        {
            this.context = context;
            this.userManager = userManager;
        }
        public async Task<ActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            // Get a list of users in the role
            var usersWithPermission = userManager.GetUsersInRoleAsync(Role.Staff).Result;
            // Then get a list of the ids of these users
            var idsWithPermission = usersWithPermission.Select(u => u.Id);
            var modelcoor = await userManager.Users
                .Where(u => idsWithPermission.Contains(u.Id))
                .Where(u => u.DepartmentId == user.DepartmentId)
                .Select(u => new StatisticalAnalysisViewModel()
                {
                    ideaCount = u.Ideas.Count,
                    reactCount = u.Reacts.Where(r => r.Type != ReactType.Null).Count(),
                    commentCount = u.Comments.Count,
                    Id = u.Id,
                    Email = u.Email,
                    DepartmentName = u.Department.Name
                })
                .OrderByDescending(u => u.ideaCount)
                .ThenByDescending(u => u.reactCount)
                .ToListAsync();
            var modelmng = await userManager.Users
                .Where(u => idsWithPermission.Contains(u.Id))
                .Select(u => new StatisticalAnalysisViewModel()
                {
                    ideaCount = u.Ideas.Count,
                    reactCount = u.Reacts.Where(r => r.Type != ReactType.Null).Count(),
                    commentCount = u.Comments.Count,
                    Id = u.Id,
                    Email = u.Email,
                    DepartmentName = u.Department.Name
                })
                .OrderBy(u => u.DepartmentName)
                .ThenByDescending(u => u.ideaCount)
                .ToListAsync();

            if (User.IsInRole(Role.Manager))
            {
                return View(modelmng);
            }
            else if (User.IsInRole(Role.Coordinator))
            {
                return View(modelcoor);
            }
            else
                return Unauthorized();
        }
    }
}
