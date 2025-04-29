using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtils.Utils.Tasks
{
    public class ThreadPerTaskScheduler : TaskScheduler
    {
        private readonly string threadName;
        private readonly ThreadPriority priority;

        public ThreadPerTaskScheduler(string threadName, ThreadPriority priority)
        {
            this.threadName = threadName;
            this.priority = priority;
        }

        /// <summary>Gets the tasks currently scheduled to this scheduler.</summary>
        /// <remarks>This will always return an empty enumerable, as tasks are launched as soon as they're queued.</remarks>
        protected override IEnumerable<Task> GetScheduledTasks() => Enumerable.Empty<Task>();

        /// <summary>Starts a new thread to process the provided task.</summary>
        /// <param name="task">The task to be executed.</param>
        protected override void QueueTask(Task task) => new Thread(() => TryExecuteTask(task))
        {
            Name = CreateThreadName(),
            Priority = priority,
            IsBackground = true,
        }.Start();

        private int counter = 0;

        private string CreateThreadName() => threadName + "_" + Interlocked.Increment(ref counter);

        /// <summary>Runs the provided task on the current thread.</summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="taskWasPreviouslyQueued">Ignored.</param>
        /// <returns>Whether the <paramref name="task"/> could be executed on the current thread.</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => TryExecuteTask(task);
    }

}
