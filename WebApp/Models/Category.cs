﻿using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public IEnumerable<Idea>? Ideas { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? FinalDueDate { get; set; }
    }
}
