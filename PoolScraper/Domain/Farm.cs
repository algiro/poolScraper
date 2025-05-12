using PoolScraper.View;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace PoolScraper.Domain
{
    public interface IFarm
    {
        string Id { get; }
        string Description { get; }
    }
    
    public static class Farm
    {
        public static IFarm Dubai => Create("Dubai", "Dubai");
        public static IFarm Ethiopia => Create("Ethiopia", "Ethiopia");
        public static IFarm Myrig => Create("Myrig", "Myrig");
        public static IFarm UNKNOWN => Create("UNKNOWN", "Unknown");

        public static IFarm[] DEFAULT_FARMS = { Dubai,Ethiopia, Myrig,UNKNOWN };

        private static IFarmStore _farmStore = FarmStore.Create(DEFAULT_FARMS);
        public static void UpdateStore(IEnumerable<IFarm> farms)
        {
            _farmStore = FarmStore.Create(farms);
        }
        public static FarmDTO AsFarmDTO(this IFarm farm) => new FarmDTO(farm.Id, farm.Description);
        public static IFarm Create(string id, string description)
        {
            return new DefaultFarm(id, description);
        }
        private class DefaultFarm : IFarm
        {
            public DefaultFarm(string id, string description)
            {
                Id = id;
                Description = description;
            }
            public string Id { get; }
            public string Description { get; }
        }

        private static (string pattern, IFarm farm)[]  farmPatterns = 
        {
                    // Key patterns, case-insensitive
                    ("^\\d*d", Farm.Dubai),           // starts with 'd' only
                    ("^\\d*eth", Farm.Ethiopia),      // starts with 'eth'
                    ("^\\d*mr", Farm.Myrig),          // starts with 'mr'
        };
        public static bool TryGet(string farmId, [NotNullWhen(true)] out IFarm? farm) => _farmStore.TryGetFarm(farmId, out farm);

        public static bool TryGetFarm(string workerName, out IFarm farm)
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
