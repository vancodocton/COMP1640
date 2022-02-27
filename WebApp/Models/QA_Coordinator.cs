using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class QA_Coordinator : ApplicationUser
    {
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}
