using System.ComponentModel.DataAnnotations;

namespace lms.api.Models.RequestModels
{
    public class CreateEmployeeRequest
    {
        [Required]
        public long EmployeeId { get; set; }

        [Required]
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(10)]
        public string MobileNumber { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string? Country { get; set; }

        [Required]
        public string? City { get; set; }

        [Required]
        public string? State { get; set; }

        [Required]
        public string? Street { get; set; }

        [Required]
        public string? HomeNo { get; set; }

        [Required]
        public string DOB { get; set; }

        [Required]
        public string DateOfJoining { get; set; }

        public long? DepartmentId { get; set; }
        public int Active { get; set; } = 1;
    }
}
