﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WebApp.Areas.Identity.Pages.Account;

namespace WebApp.ViewModels
{
    public class AccountCreateViewModel
    {
        public RegisterModel.InputModel Input { get; set; } = new();

        [StringLength(50)]
        [Required]
        public string? FullName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [StringLength(80)]
        public string? Address { get; set; }

        [Required]
        public string? Role { get; set; }

        [ValidateNever]
        public List<SelectListItem>? Roles { get; set; }
    }
}
