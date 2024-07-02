using System.ComponentModel.DataAnnotations;

namespace lms.api.Models.RequestModels
{
    public record LoginRequest
    {
        [Required]
        public long EmployeeId {  get; set; }
        [Required]
        public string Password { get; set; }
    }
}
