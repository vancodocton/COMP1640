using App.Web.Models;

namespace App.Web.ViewModels
{
    public class StatisticalAnalysisViewModel
    {
        public string? Id { get; set; }

        public string? Email { get; set; }
        public string? DepartmentName { get; set; }
        public ApplicationUser? User { get; set; }
        public int ideaCount { get; set; }
        public int commentCount { get; set; }
        public int reactCount { get; set; }

    }
}