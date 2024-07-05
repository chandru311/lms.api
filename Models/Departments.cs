using System.ComponentModel.DataAnnotations;

namespace lms.api.Models
{
    public class Departments
    {
        [Key]
        public long DepartmentId { get; set; }

        [Required]
        public string DepartmentName { get; set; }

        public long? ManagerId { get; set; }
        public string? DepartmentHead { get; set; }
        public long? EmployeesCount { get; set; }
        public int Active { get; set; } = 1;
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }

    }
}
