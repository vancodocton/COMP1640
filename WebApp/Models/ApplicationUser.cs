﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        public string? FullName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [StringLength(80)]
        public string? Address { get; set; }

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        [ValidateNever]
        public ICollection<React> Reacts { get; set; } = null!;

        [ValidateNever]
        public ICollection<Idea> Ideas { get; set; } = null!;

        [ValidateNever]
        public ICollection<Comment> Comments { get; set; } = null!;
        
        [Required]
        public bool TermConfirmed { get; set; } = false;
    }
}
