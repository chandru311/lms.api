namespace lms.api.Models.ResponseModels
{
    public record AuthenticatedToken
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public long AiId { get; set; }
        public int UserType { get; set; }
    }
}
