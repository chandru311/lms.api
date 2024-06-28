namespace lms.api.Models.ResponseModels
{
    public class UsermasterResponse
    {
        public long EmployeeId { get; set; }
        public int? UserType { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? Active { get; set; }
    }
}
