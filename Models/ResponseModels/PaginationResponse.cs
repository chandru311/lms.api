namespace lms.api.Models.ResponseModels
{
    public class PaginationResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPage { get; set; }
        public int CurrentPage { get; set; }
        public T Model { get; set; }
    }
}
