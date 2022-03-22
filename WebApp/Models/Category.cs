using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class Category
    {         
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public IEnumerable<Idea> Ideas { get; set; } = null!;

        public DateTime? DueDate { get; set; }

        public DateTime? FinalDueDate { get; set; }

        [NotMapped]
        public TimeZoneInfo UserTz { get; set; } = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");

        [NotMapped]
        public DateTime? DueDateByUserTz
        {
            get
            {
                if (DueDate == null)
                    return null;
                else
                    return TimeZoneInfo.ConvertTimeFromUtc(DueDate.Value, UserTz);
            }
            set
            {
                if (value == null)
                    DueDate = null;
                else
                    DueDate = TimeZoneInfo.ConvertTimeToUtc(value.Value, UserTz);
            }
        }

        [NotMapped]
        public DateTime? FinalDueDateByUserTz
        {
            get
            {
                if (FinalDueDate == null)
                    return null;
                else
                    return TimeZoneInfo.ConvertTimeFromUtc(FinalDueDate.Value, UserTz);
            }
            set
            {
                if (value == null)
                    FinalDueDate = null;
                else
                    FinalDueDate = TimeZoneInfo.ConvertTimeToUtc(value.Value, UserTz);
            }
        }
    }
}
