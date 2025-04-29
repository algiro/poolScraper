using CommonUtils.Scheduler;
using CommonUtils.Utils;

namespace CommonUtils.Scheduler.Ports
{
    public static class DailyScheduledTaskInfoExtension
    {
        public static DailyScheduledTaskInfoDto AsDto(this IDailyScheduledTaskInfo scheduledTaskInfo)
        {
            return new DailyScheduledTaskInfoDto
            {
                Name = scheduledTaskInfo.Name,
                IsStopped = scheduledTaskInfo.IsStopped,
                IsRunning = scheduledTaskInfo.IsRunning,
                LastExecutionTime = scheduledTaskInfo.LastExecutionTime != null ? scheduledTaskInfo.LastExecutionTime.Value.FormatDefaultTimeStamp() : "",
                LastExecutionDetails = scheduledTaskInfo.LastExecutionDetails ?? "-",
                ScheduledTime = scheduledTaskInfo.ScheduledTime.ToString(),
                BeforeNextExecution = scheduledTaskInfo.TimeBeforeNextExecution().ToString()    
            };
        }
    }
}
