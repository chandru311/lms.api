using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace lms.api.Models.RequestModels
{
    public class CreateManagerRequest
    {
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
        public string Password { get; set; }

        [Required]
        public DateOnly DOB { get; set; }
        [Required]
        public DateOnly DateOfJoining { get; set; }
    }
}
