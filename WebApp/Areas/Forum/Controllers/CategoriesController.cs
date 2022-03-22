using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WebApp.Data;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Authorize(Roles = Role.Manager)]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Category.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.DueDate > model.FinalDueDate)
                    ModelState.AddModelError(
                        key: "FinalDueDate",
                        errorMessage: "The final due Date cannot be earlier than the due date.");
                else
                {
                    var category = new Category()
                    {
                        Name = model.Name,
                        Description = model.Description,
                        DueDateByUserTz = model.DueDate,
                        FinalDueDateByUserTz = model.FinalDueDate,
                    };

                    _context.Category.Add(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(model);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }
            var model = new CategoryViewModel()
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DueDate = category.DueDateByUserTz,
                FinalDueDate = category.FinalDueDateByUserTz
            };
            return View(model);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel model)
        {

            if (ModelState.IsValid)
            {
                if (model.DueDate > model.FinalDueDate)
                    ModelState.AddModelError("FinalDueDate", "The final due Date cannot be earlier than the due date.");
                else
                {
                    var category = await _context.Category.FindAsync(model.Id);
                    if (category == null)
                    {
                        return NotFound();
                    }
                    category.Name = model.Name;
                    category.Description = model.Description;
                    category.DueDateByUserTz = model.DueDate;
                    category.FinalDueDateByUserTz = model.FinalDueDate;

                    _context.Category.Attach(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Category.FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return RedirectToAction(nameof(Index));

            var idea = await _context.Idea.FirstOrDefaultAsync(i => i.CategoryId == category.Id);

            if (idea == null)
            {
                _context.Category.Remove(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
                ModelState.AddModelError("", "Category contains idea(s), cannot delete it!");

            return View(category);
        }

        private bool CategoryExists(int id)
        {
            return _context.Category.Any(e => e.Id == id);
        }
    }
}
