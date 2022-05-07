using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Web.Data;
using App.Core.Entities;
using App.Web.ViewModels;

namespace App.Web.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Authorize(Roles = $"{Role.Staff},{Role.Coordinator},{Role.Manager}")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly int pageSize = 5;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager
            )
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult> Index(ForumSearch search)
        {
            var user = await userManager.GetUserAsync(User);

            var ideas = context.Idea
                .Include(i => i.Category)
                .Include(i => i.User)
                .Include(i => i.Comments
                    .OrderByDescending(c => c.Id)
                    .Take(2)
                    .OrderBy(c => c.Id)
                    )
                .ThenInclude(c => c.User)
                .AsSplitQuery();
                //.Select(i => new Idea()
                //{
                //    Id = i.Id,
                //    UserId = i.UserId,
                //    User = new ApplicationUser()
                //    {
                //        Id = i.UserId,
                //        UserName = i.User.UserName,
                //        Email = i.User.Email,
                //        DepartmentId = i.User.DepartmentId
                //    },
                //    CategoryId = i.CategoryId,
                //    Category = new Category()
                //    {
                //        Id = i.Category.Id,
                //        Name = i.Category.Name
                //    },
                //    IsIncognito = i.IsIncognito,
                //    Title = i.Title,
                //    Content = i.Content,
                //    Comments = i.Comments,
                //    ThumbUp = i.ThumbUp,
                //    ThumbDown = i.ThumbDown,
                //    NumComment = i.NumComment,
                //    NumView = i.NumView,
                //});

            // filter by category
            Category? category = null;
            if (search.CategoryId != null)
            {
                category = await context.Category.FirstOrDefaultAsync(c => c.Id == search.CategoryId);
                if (category != null)
                    ideas = ideas.Where(i => i.CategoryId == search.CategoryId);
            }

            // filter by order
            switch (search.Order)
            {
                case null:
                case "lastest":
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
                default:
                    search.Order = null;
                    break;
            }

            // filter by department (only Manager and Staff) and load data for filter
            if (User.IsInRole(Role.Coordinator))
            {
                search.Departments.Add(
                    new SelectListItem("My Department", $"{user.DepartmentId}"));
                if (search.DepartmentId != null
                    && search.DepartmentId == user.DepartmentId)
                {
                    ideas = ideas.Where(i => i.User.DepartmentId == user.DepartmentId);
                }
            }
            else if (User.IsInRole(Role.Manager))
            {
                var departments = await context.Department
                    .Select(d => new SelectListItem(d.Name, d.Id.ToString()))
                    .ToListAsync();
                search.Departments.AddRange(departments);

                if (search.DepartmentId != null && await context.Department.AnyAsync(i => i.Id == search.DepartmentId))
                {
                    ideas = ideas.Where(i => i.User.DepartmentId == search.DepartmentId);
                }
            }

            // load data to view model
            search.Categories = await context.Category.ToListAsync();
            var model = new ForumViewModel
            {
                User = user,
                Ideas = await PaginatedList<Idea>.CreateAsync(ideas, search.Page, pageSize),
                Search = search,
                Category = category,
            };

            return View(model);
        }
    }
}
