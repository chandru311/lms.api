using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace lms.api.Models
{
    public class Departments
    {
        [Key]
        public long DepartmentId { get; set; }

        [AllowNull]
        public long? DepartmentHeadId { get; set; }

        [Required]
        public string DepartmentName { get; set; }

        [Required]
        public string DepartmentDescription { get; set; }
#nullable enable
        [AllowNull]
        public string? DepartmentHead { get; set; }
#nullable disable
        [AllowNull]
        public long? EmployeesCount { get; set; }

        public int Active { get; set; } = 1;

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
