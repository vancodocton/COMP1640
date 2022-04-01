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
                Categories = await context.Category
                    .Where(c => c.DueDate == null || DateTime.UtcNow <= c.DueDate)
                    .Select(c => new SelectListItem()
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToListAsync(),
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
                var category = context.Category.FirstOrDefault(c => c.Id == model.CategoryId);

                if (category == null)
                {
                    ModelState.AddModelError(nameof(model.CategoryId), "The selected category has been deleted or does not exist. Please choose another idea.");
                }
                else if (DateTime.UtcNow > category.DueDate)
                {
                    ModelState.AddModelError("", $"The due date of the category '{category.Name}' is over. Cannot submit idea. Please choose another idea.");
                }
                else
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

                    if (model.Documents != null)
                    {
                        idea.FileOnFileSystems = new List<FileOnFileSystem>();

                        foreach (var file in model.Documents)
                        {
                            var basePath = Path.Combine(Directory.GetCurrentDirectory() + $"\\Files\\{category.Id}\\");

                            bool basePathExists = Directory.Exists(basePath);
                            if (!basePathExists) Directory.CreateDirectory(basePath);

                            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                            var filePath = Path.Combine(basePath, Path.GetRandomFileName());
                            var extension = Path.GetExtension(file.FileName);

                            if (!System.IO.File.Exists(filePath))
                            {
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }
                                var fileModel = new FileOnFileSystem
                                {
                                    FileType = file.ContentType,
                                    Extension = extension,
                                    Name = fileName,
                                    FilePath = filePath,

                                };

                                idea.FileOnFileSystems.Add(fileModel);
                            }
                        }
                    }

                    await context.Idea.AddAsync(idea);
                    var count = await context.SaveChangesAsync();

                    var coors = (await userManager.GetUsersInRoleAsync(Role.Coordinator))
                        .Where(c => c.DepartmentId == user.DepartmentId)
                        .ToList();

                    var url = Url.ActionLink("Idea", "Forum", new { id = idea.Id });

                    foreach (var coor in coors)
                    {
                        if (coor.EmailConfirmed)
                            await sender.SendEmailAsync(coor.Email, "New Idea is created by your department staff", $"<a href={HtmlEncoder.Default.Encode(url!)}>Idea</a>");
                    }

                    return RedirectToAction(nameof(Index), new { id = idea.Id });
                }
            }

            model.Categories = await context.Category
                .Where(c => c.DueDate == null || DateTime.UtcNow <= c.DueDate)
                .Select(c => new SelectListItem()
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();

            return View(model);
        }
        public async Task<IActionResult?> DownloadFileFromFileSystem(int id)
        {
            var file = await context.FileOnFileSystem
                .FirstOrDefaultAsync(x => x.Id == id);

            if (file == null)
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(file.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, file.FileType, file.Name + file.Extension);
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null) return BadRequest();

            var model = await context.Idea
                .Include(i => i.Category)
                .Include(i => i.User)
                .Include(i => i.FileOnFileSystems)
                .Select(i => new Idea()
                {
                    Id = i.Id,
                    UserId = i.UserId,
                    Title = i.Title,
                    Content = i.Content,
                    ThumbUp = i.ThumbUp,
                    ThumbDown = i.ThumbDown,
                    Category = i.Category,
                    IsIncognito = i.IsIncognito,
                    User = new ApplicationUser()
                    {
                        Id = i.UserId,
                        UserName = i.User!.UserName,
                    },
                    FileOnFileSystems = i.FileOnFileSystems
                }).FirstOrDefaultAsync(i => i.Id == id);

            if (model == null) return BadRequest();

            model.Comments = await context.Comment
                .Include(c => c.User)
                .Select(c => new Comment()
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    User = new ApplicationUser()
                    {
                        UserName = c.User!.UserName,
                        DepartmentId = c.User!.DepartmentId
                    },
                    IdeaId = c.IdeaId,
                    Content = c.Content,
                }).Where(i => i.IdeaId == model.Id)
                .ToListAsync();

            var user = await userManager.GetUserAsync(User);
            {
                ViewData["UserId"] = user.Id;
            }
            {
                var department = await context.Department
                    .SingleOrDefaultAsync(d => d.Id == user.DepartmentId);
                ViewData["UserDepartmentId"] = department?.Id;
            }

            return View(model);
        }


        [HttpGet]
        [Authorize(Roles = Role.Coordinator)]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await userManager
                .GetUserAsync(User);

            var department = await context.Department
                .FirstAsync(d => d.Id == user.DepartmentId);

            var idea = await context.Idea
                .Include(i => i.User)
                .Include(i => i.Category)
                .Include(i => i.FileOnFileSystems)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (idea == null)
            {
                return NotFound();
            }

            if (idea.User.DepartmentId == department.Id && ( idea.Category.FinalDueDate == null || DateTime.UtcNow < idea.Category.FinalDueDate))
            {
                return View(idea);
            }
            else
                return Unauthorized();

        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = Role.Coordinator)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await userManager.GetUserAsync(User);

            var department = await context.Department
                .FirstAsync(d => d.Id == user.DepartmentId);

            var idea = await context.Idea
                .Include(i => i.User)
                .Include(i => i.Category)
                .Include(i => i.FileOnFileSystems)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (idea == null)
            {
                return RedirectToAction(nameof(Index), "Home");
            }
            
            if (idea.User.DepartmentId == department.Id && idea.Category.FinalDueDate == null || DateTime.UtcNow < idea.Category.FinalDueDate)
            {
                context.Idea.Remove(idea);
                await context.SaveChangesAsync();
            }
            else
            {
                ModelState.AddModelError("", "Cannot delete idea of staff belonging to other departments");
                return View("Delete", idea);
            }

            /*
            var file = await context.FileOnFileSystem
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (file == null)
                    return NotFound();

                if (System.IO.File.Exists(file.FilePath))
                {
                    System.IO.File.Delete(file.FilePath);
                }
                context.FileOnFileSystem.Remove(file);
                context.SaveChanges();
                TempData["Message"] = $"Removed {file.Name + file.Extension} successfully from File System.";
            */
                return RedirectToAction(nameof(Index), "Home");

        }
    }
}
