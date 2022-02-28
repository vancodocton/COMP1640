﻿using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Idea
    {
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Content { get;set; }

        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
