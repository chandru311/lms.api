using System.ComponentModel;

namespace lms.api.Types
{
    public enum UserTypes
    {
        [Description("Manager")]
        Manager = 1,
        [Description("Employee")]
        Employee = 2,
    }
}
