using PoolScraper.Config;
using PoolScraper.View;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace PoolScraper.Domain
{
    public interface IFarm
    {
        int Id { get; }
        string Name { get; }
        string Location { get; }
        bool IsCompatible(string workerName);
    }
    
    public static class Farm
    {
        public static IFarm UNKNOWN => Create(-1, "UNKNOWN", "^UNKNOWN", "Unknown");

        private static IFarmStore _farmStore = FarmStore.Create([]);
        public static void UpdateStore(IEnumerable<IFarm> farms)
        {
            _farmStore = FarmStore.Create(farms);
        }
        public static FarmDTO AsFarmDTO(this IFarm farm) => new FarmDTO(farm.Id, farm.Name, farm.Location);
        public static IFarm Create(int id, string name, string regEx, string location)
        {
            return new DefaultFarm(id, name, regEx, location);
        }
        private class DefaultFarm : IFarm
        {
            public DefaultFarm(int id, string name, string regEx, string location)
            {
                Id = id;
                Name = name;
                Location = location;
                RegExPattern = regEx;
            }
            public int Id { get; }
            public string Location { get; }
            public string Name { get; }
            private string RegExPattern { get; }

            public bool IsCompatible(string workerName)
            {
                var suffix = Worker.GetWorkerSuffix(workerName);
                if (suffix == null)
                    return false;

                return Regex.IsMatch(suffix.ToLower(), RegExPattern);
            }
        }

        public static bool TryGet(int farmId, [NotNullWhen(true)] out IFarm? farm) => _farmStore.TryGetFarm(farmId, out farm);

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
