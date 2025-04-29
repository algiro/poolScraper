namespace PoolScraper.Service.Utils
{
    public class RecurringTask(ILogger<RecurringTask> logger, string taskId, Func<Task> workAction, TimeSpan interval)
    {
        private readonly CancellationTokenSource _cts = new();

        public async Task StartAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await DoWorkAsync();
                await Task.Delay(interval, _cts.Token);
            }
        }

        private async Task DoWorkAsync()
        {
            try
            {
                await workAction();
            }
            catch (TaskCanceledException)
            {
                logger.LogWarning($"Task {taskId} was canceled.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while executing the recurring task {taskId}");
            }

        }

        public void Stop()
        {
            _cts.Cancel();
        }
    }

}
