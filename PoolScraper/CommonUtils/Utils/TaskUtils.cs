using CommonUtils.Utils.Tasks;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtils.Utils
{
    public static class TaskUtils
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TaskUtils));
        public static Task<Task> StartAsync(this Action Action, CancellationToken cancellationToken = default,
            TaskCreationOptions taskCreationOptions = TaskCreationOptions.DenyChildAttach, TaskScheduler? taskScheduler = null, TimeSpan? delay = null)
            => Action.AsTask().StartAsync(cancellationToken, taskCreationOptions, taskScheduler ?? TaskScheduler.Default, delay);

        public static Task<Task> StartAsync(this Func<Task> CreateTask, CancellationToken cancellationToken = default,
            TaskCreationOptions taskCreationOptions = TaskCreationOptions.DenyChildAttach, TaskScheduler? taskScheduler = null, TimeSpan? delay = null)
            => Task.Factory.StartNew(() => RunProtected(CreateTask.Delay(delay), cancellationToken), cancellationToken, taskCreationOptions, taskScheduler ?? TaskScheduler.Default);

        public static Task<Task<Result?>> StartAsync<Result>(this Func<Task<Result>> CreateTask, CancellationToken cancellationToken = default,
            TaskCreationOptions taskCreationOptions = TaskCreationOptions.DenyChildAttach, TaskScheduler? taskScheduler = null, TimeSpan? delay = null)
            => Task.Factory.StartNew(() => RunProtected(CreateTask.Delay(delay), cancellationToken), cancellationToken, taskCreationOptions, taskScheduler ?? TaskScheduler.Default);

        private static Func<Task> Delay(this Func<Task> CreateTask, TimeSpan? delay)
        {
            async Task CreateDelayedTask()
            {
                await Task.Delay(delay.Value);
                await CreateTask();
            }
            return delay == null ? CreateTask : CreateDelayedTask;
        }

        private static Func<Task<Result>> Delay<Result>(this Func<Task<Result>> CreateTask, TimeSpan? delay)
        {
            async Task<Result> CreateDelayedTask()
            {
                await Task.Delay(delay.Value);
                return await CreateTask();
            }
            return delay == null ? CreateTask : CreateDelayedTask;
        }

        public static Task DoNothingAsync() => Task.CompletedTask;

        public static Task DoNothingAsync<T>(T _) => Task.CompletedTask;

        public static Task DoNothingAsync<T1, T2>(T1 _1, T2 _2) => Task.CompletedTask;

        public static Func<Task> AsTask(this Action? action) => () => { action?.Invoke(); return Task.CompletedTask; };
        public static Func<T, Task> AsTask<T>(this Action<T>? action) => t => { action?.Invoke(t); return Task.CompletedTask; };
        public static Func<T, Q, Task> AsTask<T, Q>(this Action<T, Q>? action) => (t, q) => { action?.Invoke(t, q); return Task.CompletedTask; };

        public static Task ForEachSequentialAsync(this IEnumerable<Func<Task>> enumerable, CancellationToken cancellationToken = default)
            => enumerable.ForEachSequentialAsync(t => t.Invoke(), cancellationToken);

        public static async Task ForEachSequentialAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> action, CancellationToken cancellationToken = default)
        {
            foreach (var item in enumerable)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await action(item);
            }
        }

        public static Task ForEachParallelAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> action, CancellationToken cancellationToken = default)
            => Task.WhenAll(enumerable.Select(t => RunProtected(() => action(t), cancellationToken)));

        public static async void FireAndForget(this Task task)
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception e)
            {
                log.Error($"Got error for async Task:{task}",e );
            }
        }

        public static Func<Task> GetProtected(this Func<Task> GetTask, CancellationToken cancellationToken = default) => () => RunProtected(GetTask, cancellationToken);

        public static async Task RunProtected(this Func<Task> GetTask, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await GetTask();
            }
            catch (Exception e)
            {
                log.Error($"Got error for async Task",e );
            }
        }

        public static Func<Task<Result?>> GetProtected<Result>(this Func<Task<Result>> GetTask, CancellationToken cancellationToken = default) => () => GetTask.RunProtected(cancellationToken);

        public static async Task<Result?> RunProtected<Result>(this Func<Task<Result>> GetTask, CancellationToken cancellationToken = default)
        {
            Result? result = default;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                result = await GetTask();
            }
            catch (Exception e)
            {
                log.Error($"Got error for async Task",e);
            }
            return result;
        }

        public static async Task Repeat(this Func<Task> GetTask, int count, CancellationToken cancellationToken = default) => await Enumerable.Repeat(GetTask, count).ForEachSequentialAsync(cancellationToken);

        public static async Task<Task> RepeatRun(this Func<Task> GetTask, TimeSpan every, TimeSpan? startAfter = null, CancellationToken cancellationToken = default)
        {
            startAfter ??= every;
            async Task RepeatTask()
            {
                await Task.Delay(startAfter.Value, cancellationToken);
                while (!cancellationToken.IsCancellationRequested)
                {
                    await RunProtected(GetTask, cancellationToken);
                    await Task.Delay(every, cancellationToken);
                }
            }
            return await StartAsync(RepeatTask, cancellationToken);
        }

        public static Task RunUntilCanceled(this CancellationToken cancellationToken, Action action)
            => Task.WhenAny(Task.Run(action, cancellationToken), cancellationToken.AwaitCancelled<bool>());

        public static async Task<T> RunUntilCanceled<T>(this CancellationToken cancellationToken, Func<T> GetValue)
            => await await Task.WhenAny(Task.Run(() => GetValue(), cancellationToken), cancellationToken.AwaitCancelled<T>());

        public static async Task<T> RunUntilCanceled<T>(this Task<T> task, CancellationToken cancellationToken)
            => await await Task.WhenAny(task, cancellationToken.AwaitCancelled<T>());

        private static Task<T> AwaitCancelled<T>(this CancellationToken cancellationToken)
            => Task.Run<T>(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(int.MaxValue, cancellationToken);
                }
                return default!;
            }, cancellationToken);

        public static async Task<T> TimeoutAfter<T>(this Task<T> task, int millisecondsTimeout)
        {
            if (millisecondsTimeout <= 0)
            {
                return await task; // no timeout wait in this case
            }
            else if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout)))
            {
                return await task;
            }
            else
            {
                throw new TimeoutException();
            }
        }

        private static readonly ConcurrentDictionary<(string ThreadName, ThreadPriority ThreadPriority), ThreadPerTaskScheduler> schedulerDictionary = new();

        public static Task<Task> RunOnDedicatedThread(string threadName, ThreadPriority priority, Func<Task> GetTask, CancellationToken cancellationToken)
            => Task.Factory.StartNew(async () => await GetTask(), cancellationToken, TaskCreationOptions.LongRunning, GetThreadPerTaskScheduler(threadName, priority));

        public static Task RunOnDedicatedThread(string threadName, ThreadPriority priority, Action Action, CancellationToken cancellationToken)
            => Task.Factory.StartNew(Action, cancellationToken, TaskCreationOptions.LongRunning, GetThreadPerTaskScheduler(threadName, priority));

        private static TaskScheduler GetThreadPerTaskScheduler(string threadName, ThreadPriority priority)
            => schedulerDictionary.GetOrAdd((threadName, priority), _ => new ThreadPerTaskScheduler(threadName, priority));

        public static async Task<bool> DelayUntil(Func<bool> Predicate, int timeout, int startDelay = 0)
        {
            const int delayTime = 50;
            int time = 0;
            bool result;
            if (startDelay > 0) await Task.Delay(startDelay);
            while (!(result = Predicate()) && time <= timeout)
            {
                await Task.Delay(delayTime);
                time += delayTime;
            }
            return result;
        }

        public static async Task<N> RepeatUntil<N>(Func<N> Func, Func<N, bool> Predicate, int timeout, int startDelay = 0)
        {
            const int delayTime = 50;
            int time = 0;
            N result;
            if (startDelay > 0) await Task.Delay(startDelay);
            while (!(Predicate(result = Func())) && time <= timeout)
            {
                await Task.Delay(delayTime);
                time += delayTime;
            }
            return result;
        }

    }
}
