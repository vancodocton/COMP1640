using System.ComponentModel.DataAnnotations;

namespace App.Web.ViewModels
{
    public class UserViewCategory
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

    }
}
