using System.ComponentModel.DataAnnotations;

namespace App.Web.Models
{
    public class Department
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public ICollection<ApplicationUser> Users { get; set; } = null!;
    }
}
