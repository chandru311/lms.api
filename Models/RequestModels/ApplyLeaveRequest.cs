using lms.api.Types;
using System.ComponentModel.DataAnnotations;

namespace lms.api.Models.RequestModels
{
    public class ApplyLeaveRequest
    {
        [Required]
        public string LeaveType { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }

        [Required]
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        [Required]
        public long EmployeeId { get; set; }
    }
}
