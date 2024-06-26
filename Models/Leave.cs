using lms.api.Types;

namespace lms.api.Models
{
    public class Leave
    {
        public int Id { get; set; }
        public long EmployeeId { get; set; }
        public string LeaveType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Reason { get; set; }
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
}
