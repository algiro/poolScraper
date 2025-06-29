
namespace PoolScraper.Persistency.Utils
{
    public interface IMongoUtils
    {
        IEnumerable<string> GetCollectionNames();
        string GetCollectionStructure(string collectionName);
        void RemoveCollection(string collectionName);
        bool CreateIndex(string collectionName, string indexName, string fieldName);
    }
}