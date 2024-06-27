using System.ComponentModel.DataAnnotations;

namespace lms.api.Models.RequestModels
{
    public class CreateManagerRequest
    {
        [Required]
        public long EmployeeId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
    }
}
