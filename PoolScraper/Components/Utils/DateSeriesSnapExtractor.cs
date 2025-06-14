using BlazorBootstrap;
using PoolScraper.Domain;

namespace PoolScraper.Components.Utils
{
    public delegate bool MatchSnapFunc<T>(ISnapshotDetailedView snapshotDetailedView,T compareElement);
    public delegate IEnumerable<T> GetSnapGroups<T>(IEnumerable<ISnapshotDetailedView> snapData);

    public class DateSeriesSnapExtractor<T>(IEnumerable<ISnapshotDetailedView> snapData, MatchSnapFunc<T> matchFunction, GetSnapGroups<T> GetGroups)
    {
        private int datasetsCount = 0;
        private int labelsCount =0;

        public List<IChartDataset> GetDefaultDataSets()
        {
            var datasets = new List<IChartDataset>();
            var groups = GetGroups(snapData);
            foreach (var group in groups)
            {
                var dataSetForModel = snapData.Where(w => matchFunction(w, group));
                datasets.Add(GetLineChartDataset(group, dataSetForModel));
            }

            return datasets;
        }

        private LineChartDataset GetLineChartDataset<T>(T group, IEnumerable<ISnapshotDetailedView> snapData)
        {
            var c = ColorUtility.CategoricalTwelveColors[datasetsCount].ToColor();

            datasetsCount += 1;

            return new LineChartDataset
            {
                Label = $"{group}",
                Data = GetData(snapData),
                BackgroundColor = c.ToRgbaString(),
                BorderColor = c.ToRgbString(),
                PointRadius = new List<double> { 5 },
                PointHoverRadius = new List<double> { 8 },
            };
        }

        private List<double?> GetData(IEnumerable<ISnapshotDetailedView> snapData)
        {
            var data = new List<double?>();
            foreach (var snap in snapData)
            {
                data.Add(snap.BasicInfo.Hashrate);
            }

            return data;
        }

        public List<string> GetDefaultDataLabels()
        {
            var labels = new List<string>();
            var dateRanges = snapData.Select(w => w.DateRange).Distinct();
            foreach (var dateRange in dateRanges)
            {
                labels.Add(dateRange.From.ToString("yyyy-MM-dd"));
            }

            return labels;
        }
    }
}
