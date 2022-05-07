using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using App.Web.Data;
using App.Web.Models;

namespace App.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.IsInRole(Role.Staff))
            {
                return RedirectToAction("Index", "Home", new { area = "Forum" });
            }

            if (User.IsInRole(Role.Admin))
            {
                return RedirectToAction("Index", "Account", new { area = "Admin" });
            }

            if (User.IsInRole(Role.Manager))
            {
                return RedirectToAction("Index", "Home", new { area = "Forum" });
            }

            if (User.IsInRole(Role.Coordinator))
            {
                return RedirectToAction("Index", "Home", new { area = "Forum" });
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}