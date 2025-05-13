namespace PoolScraper.Persistency.Utils
{
    using MongoDB.Driver;
    using System;

    public class SequenceGenerator : ISequenceGenerator
    {
        private readonly IMongoCollection<Counter> _counterCollection;

        public SequenceGenerator(ILogger logger, string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _counterCollection = database.GetCollection<Counter>("counters");
        }

        public long GetNextSequence(string sequenceName)
        {
            // Filter to find the document for the given sequence name.
            var filter = Builders<Counter>.Filter.Eq(c => c.Id, sequenceName);

            // The update increases the Value field by 1.
            var update = Builders<Counter>.Update.Inc(c => c.Value, 1);

            // Options: upsert=true ensures a new document is created if none exists,
            // and ReturnDocument.After returns the document AFTER the update.
            var options = new FindOneAndUpdateOptions<Counter>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            // Atomically increment the sequence and return the result.
            var result = _counterCollection.FindOneAndUpdate(filter, update, options);
            return result.Value;
        }
    }

}
