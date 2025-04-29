using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtils.Scheduler.Ports
{
    public class DailyScheduledTaskInfoDto
    {
        public string Name { get; set; } = "";
        public bool IsStopped { get; set; }
        public bool IsRunning { get; set; }
        public string ScheduledTime { get; set; } = "";
        public string BeforeNextExecution { get; set; } = "";
        public string LastExecutionTime { get; set; } = "";
        public string LastExecutionDetails { get; set; } = "";
    }
}
