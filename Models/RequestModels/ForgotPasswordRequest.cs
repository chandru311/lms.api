using System.ComponentModel.DataAnnotations;

namespace lms.api.Models.RequestModels
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string Email { get; set; }
    }
}
