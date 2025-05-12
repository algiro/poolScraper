using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Domain
{
    public interface IFarmStore
    {
        void AddFarm(IFarm farm);
        void RemoveFarm(string farmId);
        bool TryGetFarm(string farmId, [NotNullWhen(true)] out IFarm? farm);
    }

    public static class FarmStore
    {
        public static IFarmStore Create(IEnumerable<IFarm> farms)
        {
            return new DefaultFarmStore(farms);
        }

        private class DefaultFarmStore : IFarmStore
        {
            private readonly IDictionary<string, IFarm> _farms;
            public DefaultFarmStore(IEnumerable<IFarm> farms)
            {
                _farms = farms.ToDictionary(f => f.Id, f => f);
            }

            public void AddFarm(IFarm farm) => _farms.Add(farm.Id, farm);

            public void RemoveFarm(string farmId) => _farms.Remove(farmId);

            public bool TryGetFarm(string farmId, [NotNullWhen(true)] out IFarm? farm)
            {
                var found = _farms.TryGetValue(farmId, out farm);
                if (!found) farm = Farm.UNKNOWN;
                return found;
            }
        }
    }
}
