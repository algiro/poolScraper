namespace PoolScraper.Domain
{
    public class PoolIDs
    {
        public const string PowerPool = "POW_POOL";
    }
    public interface IPool
    {
        string ApiKey { get; }
        string PoolId { get; }
    }
    public static class Pool
    {
        public static IPool CreatePowerPool() => new DefaultPool("0803cab54344474b915b42c74b5d8d8b", PoolIDs.PowerPool);

        private readonly struct DefaultPool: IPool
        {
            public string ApiKey { get; }
            public string PoolId { get; }
            public DefaultPool(string apiKey, string poolId)
            {
                ApiKey = apiKey;
                PoolId = poolId;
            }
        }
    }
    public class Pools
    {

    }
}
