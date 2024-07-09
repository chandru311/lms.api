using System.ComponentModel.DataAnnotations;

namespace lms.api.Models.RequestModels
{
    public class CreateLeaveType
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int NoOfDays { get; set; }
    }
}
