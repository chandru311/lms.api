using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace lms.api.Models
{
    public class LeaveType
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int NoOfDays { get; set; }
        public int Active { get; set; } = 1;
        [AllowNull]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        [AllowNull]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
