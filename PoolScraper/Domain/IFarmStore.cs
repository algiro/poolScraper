using System.Diagnostics.CodeAnalysis;

namespace PoolScraper.Domain
{
    public interface IFarmStore
    {
        void AddFarm(IFarm farm);
        void RemoveFarm(int farmId);
        bool TryGetFarm(int farmId, [NotNullWhen(true)] out IFarm? farm);
        IEnumerable<IFarm> GetAllFarms();
    }

    public static class FarmStore
    {
        public static IFarmStore Create(IEnumerable<IFarm> farms)
        {
            return new DefaultFarmStore(farms);
        }

        private class DefaultFarmStore : IFarmStore
        {
            private readonly IDictionary<int, IFarm> _farms;
            public DefaultFarmStore(IEnumerable<IFarm> farms)
            {
                _farms = farms.ToDictionary(f => f.Id, f => f);
            }

            public void AddFarm(IFarm farm) => _farms.Add(farm.Id, farm);

            public IEnumerable<IFarm> GetAllFarms() => _farms.Values;

            public void RemoveFarm(int farmId) => _farms.Remove(farmId);

            public bool TryGetFarm(int farmId, [NotNullWhen(true)] out IFarm? farm)
            {
                var found = _farms.TryGetValue(farmId, out farm);
                if (!found) farm = Farm.UNKNOWN;
                return found;
            }
        }
    }
}
