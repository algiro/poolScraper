using System.Text.RegularExpressions;

namespace PoolScraper.Domain
{
    public enum Farm
    {
        Dubai,
        Ethiopia,
        Myrig,
        UNKNOWN
    }
    
    public static class FarmExtension
    {
        private static (string pattern, Farm farm)[]  farmPatterns = 
        {
                    // Key patterns, case-insensitive
                    ("^\\d*d", Farm.Dubai),           // starts with 'd' only
                    ("^\\d*eth", Farm.Ethiopia),      // starts with 'eth'
                    ("^\\d*mr", Farm.Myrig),          // starts with 'mr'
        };
        public static bool TryGetFarm(string workerName, out Farm farm)
        {
            farm = Farm.UNKNOWN;
            if (string.IsNullOrWhiteSpace(workerName))
                return false;

            var suffix = Worker.GetWorkerSuffix(workerName);
            if (suffix == null)
                return false;

            // Preprocess farm patterns for easy extensibility

            // Legacy/manual fallback (in case the above doesn't resolve, but you want backward compatibility)
            foreach (var fp in farmPatterns)
            {
                if (Regex.IsMatch(suffix.ToLower(), fp.pattern))
                {
                    farm = fp.farm;
                    return true;
                }
            }

            return false;
        }

    }
}
