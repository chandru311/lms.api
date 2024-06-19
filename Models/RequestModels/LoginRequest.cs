namespace lms.api.Models.RequestModels
{
    public record LoginRequest
    {
        public long EmployeeId {  get; set; }
        public string Password { get; set; }
    }
}
