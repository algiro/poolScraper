using System.Text.RegularExpressions;

namespace PoolScraper.Domain
{
    public enum WorkerModel
    {
        L7 = 10,
        L9 = 20,
        G11 = 30,
        T21 = 40,
        S21 = 50,
        S21XP = 60,
        UNKNOWN = 100,
    }

    public static class WorkerModelExtensions
    {
        public static bool TryGetModel(string workerName, out WorkerModel model)
        {
            model = WorkerModel.UNKNOWN;
            if (string.IsNullOrWhiteSpace(workerName))
                return false;

            var suffix = Worker.GetWorkerSuffix(workerName);
            if (suffix == null)
                return false;

            // Iterate over all model names (longest first to prevent partial matches)
            var modelNames = Enum.GetNames(typeof(WorkerModel));
            Array.Sort(modelNames, (a, b) => b.Length.CompareTo(a.Length)); // Descending by length

            foreach (string mn in modelNames)
            {
                if (mn.Equals("UNKNOWN", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Use word boundary, case-insensitive matching for model name
                var pattern = Regex.Escape(mn);
                var match = Regex.Match(suffix, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    model = (WorkerModel)Enum.Parse(typeof(WorkerModel), mn, true);
                    return true;
                }
            }

            return false;
        }

    }
}