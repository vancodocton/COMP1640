using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Encodings.Web;
using App.Web.Areas.Identity.Pages.Account;
using App.Web.Data;
using App.Web.Models;
using App.Web.ViewModels;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Role.Admin)]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly List<string> _createdRoles;

        public AccountController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IEmailSender emailSender
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _emailSender = emailSender;

            _createdRoles = new List<string>() { Role.Coordinator, Role.Staff };
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = _roleManager.Roles.ToDictionary(r => r.Id, r => r.Name);

            var users = await _userManager.Users
                .Include(u => u.UserRoles)
                .OrderBy(u => u.UserRoles.First().RoleId)
                .Select(u => new UserViewModel()
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FullName = u.FullName,
                    Roles = u.UserRoles.Select(ur => roles[ur.RoleId]).ToList()
                })
                .ToListAsync();

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new AccountCreateViewModel()
            {
                Roles = new SelectList(_createdRoles).ToList(),
                Departments = new SelectList(await _context.Department.ToListAsync(), "Id", "Name").ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!_createdRoles.Contains(model.Role))
                    ModelState.AddModelError("Role", "Cannot create accout with role" + model.Role);
                else
                {
                    var user = new ApplicationUser()
                    {
                        UserName = model.Input.Email,
                        Email = model.Input.Email,
                        Address = model.Address,
                        FullName = model.FullName,
                        BirthDate = model.BirthDate,
                        DepartmentId = model.DepartmentId,
                    };

                    var createAccountResult = await _userManager.CreateAsync(user, model.Input.Password);

                    if (createAccountResult.Succeeded)
                    {

                        var addAccountRoleResult = await _userManager.AddToRoleAsync(user, model.Role);

                        if (addAccountRoleResult.Succeeded)
                        {
                            // sent confirmation email
                            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                            var callbackUrl = Url.Action(
                                action: "ConfirmEmail",
                                controller: "Account",
                                new { area = "Identity", userId = user.Id, code, returnUrl = "/Identity/Account/Login" },
                                protocol: "https");

                            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? "")}'>clicking here</a>.");

                            return RedirectToAction(nameof(Index));
                        }
                        else
                            AddErrors(addAccountRoleResult.Errors);
                    }
                    else
                        AddErrors(createAccountResult.Errors);
                }
            }

            model.Roles = new SelectList(_createdRoles).ToList();
            model.Departments = new SelectList(await _context
                .Department
                .ToListAsync(), "Id", "Name").ToList();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPwd(string? id)
        {
            var model = new RegisterModel.InputModel();

            if (!string.IsNullOrEmpty(id))
                model.Email = (await _userManager.FindByIdAsync(id))?.Email;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPwd(RegisterModel.InputModel model)
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

        [HttpGet]
        public async Task<IActionResult> Update(string? id)
        {
            if (id == null)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return BadRequest();

            if (await _userManager.IsInRoleAsync(user, Role.Admin) || await _userManager.IsInRoleAsync(user, Role.Manager))
                return Forbid();

            var model = new AccountUpdateViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                Address = user.Address,
                DepartmentId = user.DepartmentId,
                Roles = new SelectList(_createdRoles).ToList(),
                Departments = new SelectList(await _context.Department.ToListAsync(), "Id", "Name").ToList()
            };

            var roles = await _userManager.GetRolesAsync(user);

            model.Role = roles.FirstOrDefault();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AccountUpdateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);

                if (user == null)
                    return BadRequest();

                // disable update email temporary
                //user.Email = model.Email;
                user.FullName = model.FullName;
                user.BirthDate = model.BirthDate;
                user.Address = model.Address;
                user.DepartmentId = model.DepartmentId;

                await _userManager.UpdateAsync(user);

                var currentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

                if (currentRole != null)
                    await _userManager.RemoveFromRoleAsync(user, currentRole);

                await _userManager.AddToRoleAsync(user, model.Role);

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user); ;

        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromForm(Name = "Id")] string id, [FromForm(Name = "isDeleteAll")] bool isDeleteAll)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return RedirectToAction(nameof(Index));

            if (await _userManager.IsInRoleAsync(user, Role.Admin) || await _userManager.IsInRoleAsync(user, Role.Manager))
            {
                ModelState.AddModelError("", "Permission denies. Cannot delete this account.");

            }
            else
            {
                if (isDeleteAll)
                {
                    var modifyIdeasComments = await _context.Idea
                        .Include(i => i.Comments.Where(c => c.UserId == id))
                        .Where(i => i.Comments.Any(r => r.UserId == id && i.Comments.Count > 0))
                        .ToListAsync();

                    var modifyIdeasReacts = await _context.Idea
                        .Include(i => i.Reacts.Where(c => c.UserId == id))
                        .Where(i => i.Reacts.Any(r => r.UserId == id && i.Reacts.Count > 0))
                        .ToListAsync();

                    foreach (var idea in modifyIdeasComments)
                    {
                        idea.NumComment -= idea.Comments.Count;
                        idea.Comments.Clear();
                    }

                    foreach (var idea in modifyIdeasReacts)
                    {
                        idea.ThumbUp -= idea.Reacts
                            .Where(r => r.Type == ReactType.ThumbUp)
                            .Count();
                        idea.ThumbDown -= idea.Reacts
                            .Where(r => r.Type == ReactType.ThumbDown)
                            .Count();
                        idea.NumView -= idea.Reacts.Count;
                        idea.Reacts.Clear();
                    }

                    var deletedIdeas = await _context.Idea
                        .Where(i => i.UserId == id)
                        .ToListAsync();

                    _context.Idea.RemoveRange(deletedIdeas);
                    //await _context.SaveChangesAsync();
                }

                var code = await _userManager.DeleteAsync(user);

                if (code.Succeeded)
                {
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                else
                    AddErrors(code.Errors);
                //return RedirectToAction(nameof(Index));
            }

            return View(nameof(Delete), user);
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
