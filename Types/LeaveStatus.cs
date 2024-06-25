using System.ComponentModel;

namespace lms.api.Types
{
    public enum LeaveStatus
    {
        [Description("Pending")]
        Pending = 0,

        [Description("Approved")]
        Approved = 1,

        [Description("Rejected")]
        Rejected = 2,
    }
}
