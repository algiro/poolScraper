using CommonUtils.Scheduler;

namespace CommonUtils.Scheduler
{
    public interface IScheduledTask
    {
        Task<IDailyScheduledTaskInfo> StopAsync(CancellationToken token = default);
        Task<IDailyScheduledTaskInfo> StartAsync(bool forceExecution=false,CancellationToken token = default);
        IDailyScheduledTaskInfo TaskInfo { get; }
    }
}
