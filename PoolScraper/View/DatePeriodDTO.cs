using PoolScraper.Domain;

namespace PoolScraper.View
{
    public class DatePeriodDTO(IDateRange dateRange)
    {
        public DateOnly From { get; set; } = DateOnly.FromDateTime(dateRange.From);

        public DateOnly To { get; set; } = DateOnly.FromDateTime(dateRange.To);
    }
}
