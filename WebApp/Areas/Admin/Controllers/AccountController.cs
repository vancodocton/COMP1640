using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: AccountController
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Create()
        {
            var model = new AccountCreateViewModel()
            {
                Roles = new SelectList(_roleManager.Roles, "Name").ToList()
            };

            return View(model);
        }

        // POST: AccountController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountCreateViewModel model)
        {
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                ModelState.AddModelError("", "Invalid account role");
            }
            else if (ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    UserName = model.Input.Email,
                    Email = model.Input.Email,
                    Address = model.Address,
                    BirthDate = model.BirthDate,
                    EmailConfirmed = true
                };

                var createAccountResult = await _userManager.CreateAsync(user, model.Input.Password);

                if (createAccountResult.Succeeded)
                {
                    var addAccountRoleResult = await _userManager.AddToRoleAsync(user, model.Role);

                    if (addAccountRoleResult.Succeeded)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                        AddErrors(addAccountRoleResult.Errors);
                }
                else
                    AddErrors(createAccountResult.Errors);
            }

            model.Roles = new SelectList(_roleManager.Roles, "Id", "Name").ToList();
            return View(model);
        }

        private void AddErrors(IEnumerable<IdentityError> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}
