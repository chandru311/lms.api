using System.ComponentModel;

namespace lms.api.Models.RequestModels
{
    public class PaginationRequest
    {
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public SortColumn? SortColumn { get; set; }
        public FilterCoulmn[]? FilterCoulmn { get; set; }
    }

    public class SortColumn
    {
        public string SortByColumn { get; set; }
        public SortOrder SortOrder { get; set; }
    }

    public class FilterCoulmn
    {
        public string ColumnName { get; set; }
        public string ColumnValue { get; set; }
        public FilterType FilterType { get; set; }
    }

    public enum SortOrder
    {
        Ascending,
        Descending,
    }

    public enum FilterType
    {
        EqualTo,
        NotEqualTo,
        GreaterThan,
        GreaterThanOrEqual,
        LesserThan,
        LesserThanOrEqual,
        Like
    }
}
