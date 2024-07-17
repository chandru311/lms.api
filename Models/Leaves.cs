using lms.api.Types;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace lms.api.Models
{
    public class Leaves
    {
        [Key]
        public long LeaveId { get; set; }

        [Required]
        public long AiId { get; set; }

        [Required]
        public string LeaveType { get; set; }

        [Required]
        public DateOnly FromDate { get; set; }

        [Required]
        public DateOnly ToDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string Reason { get; set; }

        [Required]
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        public long? ReViewedBy { get; set; }

#nullable enable
        [AllowNull]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        [AllowNull]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
#nullable disable
    }
}
