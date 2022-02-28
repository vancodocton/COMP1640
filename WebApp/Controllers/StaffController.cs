using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class StaffController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
