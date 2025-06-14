namespace PoolScraper.Domain
{
    public enum AppEventType
    {
        Info = 10,
        Warning = 20,
        Error = 30
    }
    public interface IAppEvent
    {
        DateTime TimeStamp { get; }
        AppEventType Type { get; }
        string Message { get; }
    }

    public static class AppEvent
    {
        public static IAppEvent Create(AppEventType appEventType, string message)
        {
            return new AppEventImpl(DateTime.UtcNow, appEventType, message);
        }
        private readonly struct AppEventImpl(DateTime dateTime, AppEventType appEventType, string message) : IAppEvent
        {
            public DateTime TimeStamp { get; } = dateTime;
            public AppEventType Type { get; } = appEventType;
            public string Message { get; } = message;
            public override string ToString() => $"{TimeStamp} - {Type}: {Message}";
        }
    }
}
