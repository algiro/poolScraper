using PoolScraper.Domain;

namespace PoolScraper.Model
{
    public class DateRangeReadModel(DateTime from, DateTime to) : IDateRange
    {
        public DateTime From { get; set; } = from;
        public DateTime To { get; set; } = to;
        public override string ToString() => $"{From} - {To}";

    }

    public static class DateRangeViewExtension
    {
        public static IDateRange AsDateRange(this DateRangeReadModel dateRangeView)
        {
            return DateRange.Create(dateRangeView.From, dateRangeView.To);
        }
    }
}
