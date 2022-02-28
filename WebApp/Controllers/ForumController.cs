using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web;



namespace WebApp.Controllers
{
    public class ForumController : Controller
    {
        public IActionResult Index()
        {

            return View();
        }


    }
}
