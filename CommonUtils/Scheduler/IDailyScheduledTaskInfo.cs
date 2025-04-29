using CommonUtils.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeProvider = CommonUtils.Utils.Clock.TimeProvider;

namespace CommonUtils.Scheduler
{
    /*
     * This interface represents a daily scheduled task.
     * A daily scheduled task is a task that is executed every day at the same time.
     */
    public interface IDailyScheduledTaskInfo
    {
        string Name { get; }
        bool IsStopped { get; }
        bool IsRunning { get; }
        TimeOnly ScheduledTime { get; }
        DateTime? LastExecutionTime { get; }
        string? LastExecutionDetails { get; }

    }

    public static class DailyScheduledTaskInfo
    {
        public static IDailyScheduledTaskInfo Create(string name,TimeOnly runningTime)
        {
            return new DefaultDailyScheduledTask(name, runningTime,false);
        }
        public static IDailyScheduledTaskInfo Disable(this IDailyScheduledTaskInfo task)
        {
            return new DefaultDailyScheduledTask(task.Name, task.ScheduledTime, isStopped:true, task.IsRunning,task.LastExecutionTime,task.LastExecutionDetails);
        }
        public static IDailyScheduledTaskInfo Enable(this IDailyScheduledTaskInfo task)
        {
            return new DefaultDailyScheduledTask(task.Name, task.ScheduledTime, isStopped:false, task.IsRunning, task.LastExecutionTime, task.LastExecutionDetails);
        }
        public static IDailyScheduledTaskInfo StartExecution(this IDailyScheduledTaskInfo task)
        {
            return new DefaultDailyScheduledTask(task.Name, task.ScheduledTime, isStopped: task.IsStopped,isRunning: true, lastExecutionTime: TimeProvider.Current.UtcNow);
        }
        public static IDailyScheduledTaskInfo EndExecution(this IDailyScheduledTaskInfo task,string? details=null)
        {
            return new DefaultDailyScheduledTask(task.Name, task.ScheduledTime, isStopped: task.IsStopped, isRunning: false, task.LastExecutionTime, details);
        }

        public static TimeSpan TimeBeforeNextExecution(this IDailyScheduledTaskInfo task) => TimeUtils.GetTimeSpanUntilNext(task.ScheduledTime);
        
        private readonly struct DefaultDailyScheduledTask(string name, TimeOnly scheduledTime,bool isStopped,bool isRunning=false, DateTime? lastExecutionTime=null,string? lastExecutionDetails=null) : IDailyScheduledTaskInfo
        {
            public string Name => name;
            public bool IsStopped => isStopped;
            public bool IsRunning => isRunning;
            public TimeOnly ScheduledTime => scheduledTime;
            public DateTime? LastExecutionTime => lastExecutionTime;
            public string? LastExecutionDetails => lastExecutionDetails;

        }
    }
}
