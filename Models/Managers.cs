using System.ComponentModel.DataAnnotations;

namespace lms.api.Models
{
    public class Managers
    {
        [Key]
        public long ManagerId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

    }
}
