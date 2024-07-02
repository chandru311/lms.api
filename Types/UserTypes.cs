using System.ComponentModel;

namespace lms.api.Types
{
    public enum UserTypes
    {
        [Description("Admin")]
        Admin = 1,
        [Description("Manager")]
        Manager = 2,
        [Description("Employee")]
        Employee = 3,
    }
}
