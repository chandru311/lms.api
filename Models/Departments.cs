using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lms.api.Models
{
    public class Departments
    {
        [Key]
        public long DepartmentId { get; set; }

        [Required]
        public string DepartmentName { get; set; }

        [Required]
        public long ManagerId { get; set; }

        [ForeignKey("ManagerId")]
        public Managers Manager { get; set; }

        public int Active { get; set; } = 1;

        public string? CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }
    }
}
