using PoolScraper.View;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace PoolScraper.Domain
{
    public interface IFarm
    {
        string Id { get; }
        string Location { get; }
        bool IsCompatible(string workerName);
    }
    
    public static class Farm
    {
        public static IFarm Dubai => Create("Dubai", "^\\d*d", "Dubai");
        public static IFarm Ethiopia => Create("Ethiopia", "^\\d*eth", "Ethiopia");
        public static IFarm Myrig => Create("Myrig", "^\\d*mr", "Russia");
        public static IFarm UNKNOWN => Create("UNKNOWN", "^UNKNOWN", "Unknown");

        public static IFarm[] DEFAULT_FARMS = { Dubai,Ethiopia, Myrig,UNKNOWN };

        private static IFarmStore _farmStore = FarmStore.Create(DEFAULT_FARMS);
        public static void UpdateStore(IEnumerable<IFarm> farms)
        {
            _farmStore = FarmStore.Create(farms);
        }
        public static FarmDTO AsFarmDTO(this IFarm farm) => new FarmDTO(farm.Id, farm.Location);
        public static IFarm Create(string id, string regEx, string location)
        {
            return new DefaultFarm(id, regEx, location);
        }
        private class DefaultFarm : IFarm
        {
            public DefaultFarm(string id, string regEx, string location)
            {
                Id = id;
                Location = location;
                RegExPattern = regEx;
            }
            public string Id { get; }
            public string Location { get; }
            private string RegExPattern { get; }

            public bool IsCompatible(string workerName)
            {
                var suffix = Worker.GetWorkerSuffix(workerName);
                if (suffix == null)
                    return false;

                return Regex.IsMatch(suffix.ToLower(), RegExPattern);
            }
        }

        public static bool TryGet(string farmId, [NotNullWhen(true)] out IFarm? farm) => _farmStore.TryGetFarm(farmId, out farm);

        public static bool TryGetFarm(string workerName, out IFarm farm)
        {
            farm = Farm.UNKNOWN;
            if (string.IsNullOrWhiteSpace(workerName))
                return false;
            
            // Legacy/manual fallback (in case the above doesn't resolve, but you want backward compatibility)
            foreach (var curFarm in _farmStore.GetAllFarms())
            {
                if (curFarm.IsCompatible(workerName))
                {
                    farm = curFarm;
                    return true;
                }
            }

            return false;
        }

    }
}
