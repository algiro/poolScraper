﻿

using CommonUtils.Utils;
using PoolScraper.Config;
using PoolScraper.Domain;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Service.Consolidation;

namespace PoolScraper.Service
{
    public class ScheduledService(ILogger<ScheduledService> logger, IInitApp initApp, IScrapingServiceClient scrapingServiceClient, 
        ISnapshotConsolidateServiceClient snapshotConsolidateServiceClient, IUptimeConsolidateServiceClient uptimeConsolidateServiceClient) : BackgroundService
    {
        private CancellationToken cancellationToken;
        //private readonly TimeOnly consolidationTime = new TimeOnly(00, 15, 0);
        private readonly TimeOnly consolidationTime = new TimeOnly(12, 10, 0);
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {   
            this.cancellationToken = stoppingToken;
            await initApp.InitAsync();
            await ScheduleScrapingAsync();
        }

        private async Task ScheduleScrapingAsync()
        {
            await TaskUtils.RepeatRun(ImportAction, every: TimeSpan.FromMinutes(1), startAfter: TimeSpan.Zero, cancellationToken: cancellationToken);
            var timeUntilNextConsolidation = TimeUtils.GetTimeSpanUntilNext(consolidationTime);
            logger.LogInformation("Next consolidation will run in {timeUntilNextConsolidation}", timeUntilNextConsolidation);
            await TaskUtils.RepeatRun(ConsolidateAction, every: TimeSpan.FromHours(24), startAfter: timeUntilNextConsolidation, cancellationToken: cancellationToken);

        }

        private async Task ImportAction()
        {
            //await scrapingServiceClient.FetchAndStoreUserDataAsync();
        }
        private async Task ConsolidateAction()
        {
            try
            {
                var yesterday = DateUtils.Today.AddDays(-1).GetEndOfDay();
                var oneWeekBefore = yesterday.AddDays(-7).GetBeginOfDay();
                await ConsolidateSnapshots(yesterday, oneWeekBefore);
                await ConsolidateUptimes(yesterday, oneWeekBefore);
                logger.LogInformation("ConsolidateAction completed for data from {startDate} to {endDate}", oneWeekBefore, yesterday);
            }
            catch (Exception ex)
            {
                logger.LogError("ConsolidateAction error: {message} {stackTrace}", ex.Message, ex.StackTrace);
            }
        }
        private async Task ConsolidateSnapshots(DateTime yesterday, DateTime oneWeekBefore)
        {
            logger.LogInformation("ConsolidateSnapshots data from {startDate} to {endDate}", oneWeekBefore, yesterday);

            var dataConsolidated = await snapshotConsolidateServiceClient.GetSnapshotDataConsolidationInfoAsync(DateRange.Create(oneWeekBefore, yesterday));
            var dayConsolidated = dataConsolidated.Where(d => d.Granularity == Granularity.Days).ToList();
            var currentDate = DateOnly.FromDateTime(oneWeekBefore);
            while (currentDate < DateOnly.FromDateTime(yesterday))
            {
                var currentDateRange = currentDate.AsDateRange();
                var hasBeenAlreadyConsolidated = dayConsolidated.Any(d => d.DateRange.Equals(currentDateRange));
                logger.LogInformation("ConsolidateSnapshots date: {date}, already consolidated: {hasBeenAlreadyConsolidated}", currentDate, hasBeenAlreadyConsolidated);
                if (!hasBeenAlreadyConsolidated)
                {
                    await snapshotConsolidateServiceClient.ConsolidateDateRange(currentDateRange);
                    logger.LogInformation("ConsolidateSnapshots done dateRange: {currentDateRange}", currentDateRange);
                }
                currentDate = currentDate.AddDays(1);
            }
        }
        private async Task ConsolidateUptimes(DateTime yesterday, DateTime oneWeekBefore)
        {
            logger.LogInformation("ConsolidateUptimes data from {startDate} to {endDate}", oneWeekBefore, yesterday);
            var dateRange = DateRange.Create(oneWeekBefore, yesterday);
            await uptimeConsolidateServiceClient.ConsolidateDays(dateRange);
        }

    }
}
