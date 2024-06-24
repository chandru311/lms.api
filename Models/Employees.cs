using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lms.api.Models
{
    public class Employees
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public long EmployeeId { get; set; }
        [Required]
        public long ManagerId { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        [MaxLength(10)]
        public string? MobileNumber { get; set; }
        [Required]
        public string? Country { get; set; }
        [Required]
        public string? City { get; set; }
        [Required]
        public string? State { get; set; }
        [Required]
        public string? DOB { get; set; }
        [Required]
        public string? DateOfJoining { get; set; }
        [Required]
        public string? Address { get; set; }
        public int Active { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
