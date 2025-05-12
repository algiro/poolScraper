using CommonUtils.Utils;
using PoolScraper.Domain.Consolidation;
using PoolScraper.Model.PowerPool;

namespace PoolScraper.Domain
{
    public interface ISnapshotWorkerStatus
    {
        string Id { get; }
        Granularity Granularity { get; }
        IWorkerId WorkerId { get; }
        IDateRange DateRange { get; }
        IWorkerBasicInfo BasicInfo { get; }
    }

    public static class SnapshotWorkerStatus
    {
        public static ISnapshotWorkerStatus Create(IWorkerId workerId, Granularity granularity, IDateRange dateRange, IWorkerBasicInfo basicInfo)
        {
            return new SnapshotWorkerStatusImpl(workerId, granularity, dateRange, basicInfo);
        }

        public static IEnumerable<ISnapshotWorkerStatus> AsSnapshotWorkerStatus(this IEnumerable<PowerPoolUser> powerPoolScrapings)
        {
            IPool powerPool = Pool.CreatePowerPool();
            List<ISnapshotWorkerStatus> snapshotWorkerStatus = new List<ISnapshotWorkerStatus>();
            DateTime? previousSnapshotTime = null;
            foreach (var document in powerPoolScrapings)
            {                
                var snapshotTime = document.FetchedAt;
                var workersStatus = document.GetAllAlgoWorkers().SelectMany(algo => algo.GetAllWorkerStatus());

                var currentSnapshotWorkerStatus = workersStatus.AsWorkersMinuteStatus(powerPool, DateRange.Create(previousSnapshotTime ?? snapshotTime, snapshotTime));
                snapshotWorkerStatus.AddRange(currentSnapshotWorkerStatus);
                previousSnapshotTime = snapshotTime;
            }
            return snapshotWorkerStatus;
        }

        public static int Weight(this ISnapshotWorkerStatus workerStatus)
        {
            if (workerStatus.Granularity == Granularity.Custom)
            {
                var diffSpan = workerStatus.DateRange.To - workerStatus.DateRange.From;
                return (int) Math.Round(diffSpan.TotalMinutes);
            }
            else
            {
                return workerStatus.Granularity.GetWeight();
            }
        }

        /// <summary>
        /// Fills the gaps between snapshots by inserting auto‐generated snapshots.
        /// For snapshots within the same WorkerId:
        ///   • The snapshots are first sorted by DateRange.From.
        ///   • In addition, if the worker’s last snapshot does not reach the global maximum “To”
        ///     found in the whole set, a trailing gap snapshot is appended.
        /// </summary>
        public static IEnumerable<ISnapshotWorkerStatus> FillTheGaps(this IEnumerable<ISnapshotWorkerStatus> snapshots)
        {
            if (snapshots is null)
                throw new ArgumentNullException(nameof(snapshots));
            if (snapshots.IsEmpty())
                return snapshots;

            // Find the global maximum "To" value across all snapshots.
            var globalMaxTo = snapshots.Max(s => s.DateRange.To);

            // Group snapshots by WorkerId. (Grouping key: PoolId and Id)
            var grouped = snapshots.GroupBy(s => (s.WorkerId.PoolId, s.WorkerId.Id));

            var result = new List<ISnapshotWorkerStatus>();

            foreach (var group in grouped)
            {
                // Work on a copy of the list (ordered by starting DateTime)
                var ordered = group.OrderBy(s => s.DateRange.From).ToList();
                var outputGroup = new List<ISnapshotWorkerStatus>();

                for (int i = 0; i < ordered.Count; i++)
                {
                    var current = ordered[i];
                    outputGroup.Add(current);

                    // Look ahead to the next snapshot within the same worker group.
                    if (i + 1 < ordered.Count)
                    {
                        var next = ordered[i + 1];
                        var currentId = Granularity.Minutes.GetId(current.DateRange.To);
                        var nextId = Granularity.Minutes.GetId(next.DateRange.From);
                        // If there is a gap between current and next...
                        if (nextId != currentId)
                        {
                            // Start of gap is exactly current's To
                            DateTime gapFrom = current.DateRange.To;
                            // End the gap one second before the next snapshot begins.
                            DateTime gapTo = next.DateRange.From.AddSeconds(-1);
                            var gapSize = gapTo - gapFrom;
                            if (gapSize >= TimeSpan.FromSeconds(60))
                            {
                                // Create the gap snapshot.
                                var gapSnapshot = Create(current.WorkerId, Granularity.Custom, DateRange.Create(gapFrom, gapTo), current.BasicInfo); // Copy the BasicInfo from the current snapshot
                                outputGroup.Add(gapSnapshot);
                            }
                        }
                    }
                }

                // For trailing gap: if the last snapshot's To is less than globalMaxTo,
                // insert a gap snapshot from the last snapshot's To to globalMaxTo.
                var last = outputGroup.Last();
                if (last.DateRange.To < globalMaxTo)
                {
                    var gapSnapshot = Create(last.WorkerId, Granularity.Custom, DateRange.Create(last.DateRange.To, globalMaxTo), last.BasicInfo); // Copy the BasicInfo from the current snapshot
                    outputGroup.Add(gapSnapshot);
                }

                // Add the processed group to the overall result.
                result.AddRange(outputGroup);
            }

            // Optionally, re-sort the final result (for example, by DateRange.From then by WorkerId).
            return result.OrderBy(s => s.WorkerId.PoolId)
                         .ThenBy(s => s.WorkerId.Id)
                         .ThenBy(s => s.DateRange.From)
                         .ToList();
        }
        public static bool HasOverlapGreaterThan(this IEnumerable<ISnapshotWorkerStatus> statuses, TimeSpan thresholdOverlap)
        {
            // Group statuses by WorkerId (using PoolId and Id as composite key)
            var groups = statuses.GroupBy(s => s.WorkerId);

            foreach (var group in groups)
            {
                // Create a list of events: +1 for a start, -1 for an end.
                var events = new List<(DateTime Time, int Delta)>();

                foreach (var status in group)
                {
                    events.Add((status.DateRange.From, +1));
                    events.Add((status.DateRange.To, -1));
                }

                // Sort events:
                // If two events have the same Time, process the start event (Delta positive) before the end event.
                var sortedEvents = events
                    .OrderBy(e => e.Time)
                    .ThenBy(e => -e.Delta)
                    .ToList();

                int activeCount = 0;
                DateTime? lastEventTime = null;
                TimeSpan totalOverlapDuration = TimeSpan.Zero;

                // Sweep over each event
                foreach (var evt in sortedEvents)
                {
                    // If we already have a "lastEventTime" recorded and there are 2 or more active intervals,
                    // accumulate the time from the previous checkpoint until now.
                    if (lastEventTime.HasValue && activeCount >= 2)
                    {
                        totalOverlapDuration += evt.Time - lastEventTime.Value;
                    }

                    activeCount += evt.Delta;
                    lastEventTime = evt.Time;
                }

                // If overlap was found greater than one minute, return immediately.
                if (totalOverlapDuration > thresholdOverlap)
                {
                    return true;
                }
            }

            return false;
        }


        private readonly struct SnapshotWorkerStatusImpl(IWorkerId workerId, Granularity granularity, IDateRange dateRange, IWorkerBasicInfo basicInfo) : ISnapshotWorkerStatus
        {
            public string Id { get; } = $"{workerId.PoolId}.{workerId.Id}.{granularity.GetId(dateRange)}";
            public Granularity Granularity { get; } = granularity;
            public IWorkerId WorkerId { get; } = workerId;
            public IDateRange DateRange { get; } = dateRange;
            public IWorkerBasicInfo BasicInfo { get; } = basicInfo;
        }
    }
}
