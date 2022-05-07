using System.ComponentModel.DataAnnotations;

namespace App.Core.Entities
{
    public class Department : BaseEntity<int>
    {
        [StringLength(50)]
        public string Name { get; set; } = null!;

        [StringLength(100)]
        public string? Description { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; } = null!;
    }
}
