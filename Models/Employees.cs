using System;
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
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? MiddleName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        [Phone]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string State { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public string DOB { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public string DateOfJoining { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string Address { get; set; } = string.Empty;
        public int Active { get; set; } = 1;

        public string? CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }
    }
}
