namespace lms.api.Models.ResponseModels
{
    public class BaseResponse<T>
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
