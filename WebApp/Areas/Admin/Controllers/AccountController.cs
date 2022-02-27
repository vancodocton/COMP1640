using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Areas.Identity.Pages.Account;
using WebApp.Data;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Areas.Admin.Controllers
{
    [Authorize(Roles = Role.Admin)]
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

        [HttpGet]
        public ActionResult ResetPwd(string? id)
        {
            var model = new RegisterModel.InputModel();

            if (!string.IsNullOrEmpty(id))
                model.Email = id;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPwd(RegisterModel.InputModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                    ModelState.AddModelError("Email", "User does not exist.");
                else if (await _userManager.IsInRoleAsync(user, Role.Admin))
                {
                    ModelState.AddModelError("", "Permission denies. Cannot reset this account password");
                }
                else
                {
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                    var resetPwdResult = await _userManager.ResetPasswordAsync(user, code, model.Password);

                    if (resetPwdResult.Succeeded)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                        AddErrors(resetPwdResult.Errors);
                }

            }

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
