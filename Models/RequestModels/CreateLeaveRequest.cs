using lms.api.Types;
using System.ComponentModel.DataAnnotations;

namespace lms.api.Models.RequestModels
{
    public class CreateLeaveRequest
    {
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
    }
}
