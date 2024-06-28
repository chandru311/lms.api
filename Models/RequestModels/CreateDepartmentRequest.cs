using System.ComponentModel.DataAnnotations;

namespace lms.api.Models.RequestModels
{
    public class CreateDepartmentRequest
    {
        [Required]
        public string DepartmentName { get; set; }
        [Required]
        public long ManagerEmployeeId { get; set; }
    }
}
