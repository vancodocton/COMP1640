using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Controllers
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
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            if (User.IsInRole(Role.Coordinator))
            {
                return RedirectToAction("Index", "Home", new { area = "Coordinator" });
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}