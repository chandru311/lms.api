using System.ComponentModel.DataAnnotations;

namespace lms.api.Models
{
    public class LeaveSum
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long AiId { get; set; }

        [Required]
        public int UserType { get; set; }

        [Required]
        public string Name { get; set; }

        public long LeavesAva { get; set; }
        public long LeavesTaken { get; set; }
        public long SickLeave { get; set; }
        public long CasualLeave { get; set; }
        public long PaidLeave { get; set; }
        public long UnpaidLeave { get; set; }
        public long Others { get; set; }
    }
}
