namespace lms.api.Models
{
    public class PublicHolidays
    {
        public long Id { get; set; }
        public string Date { get; set; }
        public string Name { get; set; }
        public int Active { get; set; } = 1;
    }
}
