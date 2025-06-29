using PoolScraper.Domain;

namespace PoolScraper.Model
{
    public class AppEventReadModel(DateTime dateTime,AppEventType appEventType, string message) : IAppEvent
    {
        public DateTime TimeStamp { get; set; } = dateTime;
        public AppEventType Type { get; set; } = appEventType;
        public string Message { get; set; } = message;
        public override string ToString() => $"{TimeStamp} - {Type}: {Message}";
    }
}
