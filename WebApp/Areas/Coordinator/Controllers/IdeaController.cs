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

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var idea = await context.Idea
                .Include(i => i.Category)
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (idea == null)
            {
                return NotFound();
            }

            return View(idea);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var user = await userManager.GetUserAsync(User);

            var department = await context.Department.SingleOrDefaultAsync(d => d.Id == user.DepartmentId);

            var idea = await context.Idea.Include(i => i.User).SingleOrDefaultAsync(i => i.Id == id);

            if(idea == null)
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            if(idea.User.DepartmentId == department.Id)
            {
                context.Idea.Remove(idea);
                await context.SaveChangesAsync();
            }
            else
                return Unauthorized();
            //var idea = await context.Idea.FindAsync(id);

            return RedirectToAction(nameof(Index), "Home");
        }
    }
}
