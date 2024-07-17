using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace lms.api.Models
{
    public class Usermaster
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long AiId { get; set; }

        [Required]
        public int UserType { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(10)]
        public string MobileNumber { get; set; }

        [Required]
        public string Password { get; set; }

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
