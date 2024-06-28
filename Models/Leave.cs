using lms.api.Types;
using System.ComponentModel.DataAnnotations;

namespace lms.api.Models
{
    public class Leave
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public long EmployeeId { get; set; }
        [Required]
        public string LeaveType { get; set; }
        [Required]
        public DateTime FromDate { get; set; }
        [Required]
        public DateTime ToDate { get; set; }
        [Required]
        public string Reason { get; set; }
        [Required]
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
}
