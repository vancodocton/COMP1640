﻿using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public IEnumerable<ApplicationUser>? Users { get; set; }
    }
}
