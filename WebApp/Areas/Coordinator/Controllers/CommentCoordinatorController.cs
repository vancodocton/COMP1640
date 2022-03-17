﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Hubs;
using WebApp.Models;

namespace WebApp.Areas.Coordinator.Controllers
{
    [Route("Forum/[controller]/[action]")]
    [Area("Coordinator")]
    [Authorize]
    public class CommentCoordinatorController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender sender;
        private readonly IHubContext<IdeaInteractHub> hubContext;

        public CommentCoordinatorController
            (
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IEmailSender sender,
            IHubContext<IdeaInteractHub> hubContext
            )
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.sender = sender;
            this.hubContext = hubContext;
        }

        // GET: Coordinator/CommentCoordinator

        // GET: Coordinator/Comment
        public IActionResult Index()
        {
            return View();
        }

        // GET: Coordinator/Comment/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }


            var comment = await dbContext.Comment
                .Include(c => c.Idea)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            // return Ok(comment.Id);
            return View(comment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await userManager.GetUserAsync(User);

            var department = await dbContext.Department.SingleOrDefaultAsync(d => d.Id == user.DepartmentId);


            var comment = await dbContext.Comment.Include(c => c.User).SingleOrDefaultAsync(m => m.Id == id);

            //var comment = await dbContext.Comment.FindAsync(id);
            dbContext.Comment.Remove(comment);
            await dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index), "Home");
        }

        private bool CommentExists(int id)
        {
            return dbContext.Comment.Any(e => e.Id == id);
        }
    }
}
