using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class UserViewCategory
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

    }
}
