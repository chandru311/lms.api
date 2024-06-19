using System.ComponentModel.DataAnnotations;

namespace lms.api.Models
{
    public class Usermaster
    {
        [Key]
        public long UId { get; set; }
        public int UserType { get; set; }

        [Required]
        public long EmployeeId { get; set; }

        [Required]
        [MaxLength(10)]
        public int MobileNumber { get; set; }

        [Required]
        public string Password { get; set; }
        public int UserStatus { get; set; }
        public int Active { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }

    }
}
