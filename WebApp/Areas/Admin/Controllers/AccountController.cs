﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        private readonly ApplicationDbContext _context;

        private List<string> _createdRoles;
        public AccountController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;

            _createdRoles = new List<string>() { Role.Coordinator, Role.Staff };
        }

        // GET: AccountController
        public async Task<ActionResult> Index()
        {
            var idea = await _context.Idea.FirstOrDefaultAsync(i => i.Id == 1);

            var model = await _userManager.Users
                .Select(u => new UserViewModel()
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FullName = u.FullName,
                    //Roles = _userManager.GetRolesAsync(u).Result
                })
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            var model = new AccountCreateViewModel()
            {
                Roles = new SelectList(_createdRoles).ToList(),
                Departments = new SelectList(await _context.Department.ToListAsync(), "Id", "Name").ToList()
            };
            return View(model);
        }

        // POST: AccountController/Create
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
                        EmailConfirmed = true,
                        DepartmentId = model.DepartmentId,
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
            }

            model.Roles = new SelectList(_createdRoles).ToList();
            model.Departments = new SelectList(await _context
                .Department
                .ToListAsync(), "Id", "Name").ToList();
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

                user.Email = model.Email;
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
        public ActionResult Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user); ;

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(ApplicationUser model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);

                if (user == null)
                    ModelState.AddModelError("Id", "User does not exist.");
                else if (await _userManager.IsInRoleAsync(user, Role.Admin) || await _userManager.IsInRoleAsync(user, Role.Manager))
                {
                    ModelState.AddModelError("", "Permission denies. Cannot delete this account password");
                }
                else
                {
                    var code = await _userManager.DeleteAsync(user);


                    if (code.Succeeded)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                        AddErrors(code.Errors);
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
