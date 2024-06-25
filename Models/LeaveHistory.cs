using lms.api.Types;

namespace lms.api.Models
{
    public class LeaveHistory
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public DateTime LeaveAppliedDate { get; set; }
        public string LeaveType { get; set; }
        public LeaveStatus Status { get; set; }
    }
}
