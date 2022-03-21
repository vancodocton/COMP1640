using WebApp.Models;

namespace WebApp.ViewModels
{
    public class StatisticalAnalysisViewModel
    {
        public string? Id { get; set; }

        public string? Email { get; set; }
        public ApplicationUser? User { get; set; }
    }
}