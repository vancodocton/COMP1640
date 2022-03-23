using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO.Compression;
using System.Text;
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
        private ILogger<CategoriesController> _logger;

        public CategoriesController(ApplicationDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Category.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

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

        public async Task<IActionResult> Edit(int id)
        {
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

        [HttpGet]
        public async Task<IActionResult> ExportAsCSV(int id, string reportType)
        {
            var category = await _context.Category.FindAsync(id);
            if (category == null)
                return NotFound();

            return reportType switch
            {
                "Ideas" => await ExportIdeasAsCSV(category),
                "Comments" => await ExportCommentsAsCSV(category),
                "Users" => await ExportUsersAsCSV(category),
                _ => BadRequest(),
            };
        }

        private async Task<IActionResult> ExportUsersAsCSV(Category category)
        {
            //_logger.LogInformation("Starting export data...");
            if (category.IsExportable)
            {
                var downloadFileName = $"{category.Name}-Users-{DateTime.UtcNow.Ticks}";

                var ideas = _context.Users
                    .IgnoreAutoIncludes()
                    .Where(u => u.Ideas.Any(i => i.CategoryId == category.Id))
                    .Select(u => new
                    {
                        u.Id,
                        u.Email,
                        u.UserName,
                        u.FullName,
                    });

                var stream = new MemoryStream();

                await WriteCsvToStreamAsync(await ideas.ToListAsync(), stream);
                // the following is return File without Archiving
                //stream.Seek(0, SeekOrigin.Begin);
                //return File(stream, "application/octet-stream", $"{downloadFileName}.csv");
                await ArchiveAddCsvEntryFromStream(downloadFileName, stream);

                //_logger.LogInformation("Send exported file to client...");
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/zip", $"{downloadFileName}.zip");
                //return File(stream, "application/octet-stream", $"{downloadFileName}.csv");
            }
            else return BadRequest();
        }

        private async Task<IActionResult> ExportIdeasAsCSV(Category category)
        {
            //_logger.LogInformation("Starting export data...");
            if (category.IsExportable)
            {
                var downloadFileName = $"{category.Name}-Ideas-{DateTime.UtcNow.Ticks}";

                var ideas = _context.Idea
                    .IgnoreAutoIncludes()
                    .Select(i => new
                    {
                        i.Id,
                        i.UserId,
                        i.IsIncognito,
                        i.Title,
                        i.Content,
                        i.CategoryId,
                        i.ThumbUp,
                        i.ThumbDown,
                        i.NumComment,
                        i.NumView,
                    })
                    .Where(i => i.CategoryId == category.Id);

                var stream = new MemoryStream();

                await WriteCsvToStreamAsync(await ideas.ToListAsync(), stream);

                await ArchiveAddCsvEntryFromStream(downloadFileName, stream);
                //_logger.LogInformation("Send exported file to client...");
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/zip", $"{downloadFileName}.zip");
            }
            else return BadRequest();
        }

        private async Task<IActionResult> ExportCommentsAsCSV(Category category)
        {
            //_logger.LogInformation("Starting export data...");
            if (category.IsExportable)
            {
                var downloadFileName = $"{category.Name}-IdeaComments-{DateTimeOffset.UtcNow.Ticks}";
                // create query source
                var comments = _context.Comment
                    .IgnoreAutoIncludes()
                    .Where(c => c.Idea.CategoryId == category.Id)
                    .Select(c => new
                    {
                        c.Id,
                        c.UserId,
                        c.IdeaId,
                        c.Content,
                    });

                var ms = new MemoryStream();

                //dont use ArchiveAddCsvEntryFromStream instead
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true, Encoding.UTF8))
                {
                    var archiveEntry = archive.CreateEntry($"{downloadFileName}.csv", CompressionLevel.Optimal);

                    using var archiveEntryStream = archiveEntry.Open();

                    await WriteCsvToStreamAsync(await comments.ToListAsync(), archiveEntryStream);
                }

                //_logger.LogInformation("Send exported file to client...");
                ms.Seek(0, SeekOrigin.Begin);
                return File(ms, "application/zip", $"{downloadFileName}.zip");
            }
            else return BadRequest();
        }

        private static async Task<Stream> ArchiveAddCsvEntryFromStream(string fileName, Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true, Encoding.UTF8))
            {
                var archiveEntry = archive.CreateEntry($"{fileName}.csv", CompressionLevel.Optimal);

                using (var archiveEntryStream = archiveEntry.Open())
                {
                    await stream.CopyToAsync(archiveEntryStream);
                }
            }

            return stream;
        }

        private static async Task<Stream?> WriteCsvToStreamAsync<T>(ICollection<T> collection, Stream? stream = null)
        {
            var isReturned = false;
            if (stream == null)
            {
                stream = new MemoryStream();
                isReturned = true;
            }

            using (var writeFile = new StreamWriter(stream, leaveOpen: true, encoding: Encoding.UTF8))
            {
                var csv = new CsvWriter(writeFile, culture: CultureInfo.InvariantCulture, leaveOpen: true);

                await csv.WriteRecordsAsync(collection);

                writeFile.Flush();
            }

            if (isReturned)
                return stream;
            return null;
        }
    }
}
