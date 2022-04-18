using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class UserViewModel
    {
        public string? Id { get; set; }

        public string? UserName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(50)]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        public ICollection<string> Roles { get; set; } = null!;
    }
}
