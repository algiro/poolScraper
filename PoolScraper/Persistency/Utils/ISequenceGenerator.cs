namespace PoolScraper.Persistency.Utils
{
    public interface ISequenceGenerator
    {
        long GetNextSequence(string sequenceName);
    }
}