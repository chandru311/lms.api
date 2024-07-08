using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace lms.api.Models
{
    public class Employees
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public long AiId { get; set; }

        [AllowNull]
        public long? AddressId { get; set; }

        [AllowNull]
        public long? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
        [Required]
        public string FirstName { get; set; }
#nullable enable
        [AllowNull]
        public string? MiddleName { get; set; }

        [AllowNull]
        public string? LastName { get; set; }
#nullable disable
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MaxLength(10)]
        public string MobileNumber { get; set; }
        [Required]
        public DateOnly DOB { get; set; }

        [Required]
        public DateOnly DateOfJoining { get; set; }

        public int Active { get; set; } = 1;

#nullable enable
        [AllowNull]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        [AllowNull]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
#nullable disable
        public ICollection<Usermaster> Usermasters { get; set; }
        public ICollection<LeaveSum> LeaveSum { get; set; }
        public ICollection<Leave> Leaves { get;}
    }
}
