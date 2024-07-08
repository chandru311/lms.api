using System.ComponentModel.DataAnnotations;

namespace lms.api.Models.RequestModels
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
