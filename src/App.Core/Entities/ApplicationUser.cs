using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace App.Core.Entities
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

        public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } = null!;

        public virtual ICollection<React> Reacts { get; set; } = null!;

        public virtual ICollection<Idea> Ideas { get; set; } = null!;

        public virtual ICollection<Comment> Comments { get; set; } = null!;
    }
}
