﻿using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using PoolScraper.Config;
using PoolScraper.Model.PowerPool;
using PoolScraper.Service.Store;

namespace PoolScraper.Persistency.Utils
{
    public class MongoUtils : IMongoUtils
    {
        private readonly ILogger _log;
        private readonly IMongoDatabase mongoDatabase;
        public MongoUtils(ILogger<PowerPoolScrapingPersistency> log, IPoolScraperConfig poolScraperConfig)
        {
            _log = log;
            _log.LogInformation("PowerPoolService C.tor with connection string: {connectionString} and database name: {databaseName}", poolScraperConfig.MongoConnectionString, poolScraperConfig.MongoDatabaseName);
            var client = new MongoClient(poolScraperConfig.MongoConnectionString);
            mongoDatabase = client.GetDatabase(poolScraperConfig.MongoDatabaseName);
        }

        public IEnumerable<string> GetCollectionNames()
        {
            try
            {
                return mongoDatabase.ListCollectionNames().ToList();
            }
            catch (Exception ex)
            {
                _log.LogError("Error retrieving collection names: {message}", ex.Message);
                return Enumerable.Empty<string>();
            }
        }
        public void RemoveCollection(string collectionName)
        {
            try
            {
                mongoDatabase.DropCollection(collectionName);
                _log.LogInformation("Collection {collectionName} removed successfully.", collectionName);
            }
            catch (Exception ex)
            {
                _log.LogError("Error removing collection {collectionName}: {message}", collectionName, ex.Message);
            }
        }

        public string GetCollectionStructure(string collectionName)
        {
            try
            {
                var collection = mongoDatabase.GetCollection<BsonDocument>(collectionName);                
                var recordCount = collection.CountDocuments(new BsonDocument());
                var firstDocument = collection.Find(new BsonDocument()).FirstOrDefault();
                var indexes = collection.Indexes.List().ToList();
                var indexesData = indexes.Select(i => i.ToBsonDocument().ToJson()).ToList();
                if (firstDocument == null)
                {
                    return "Collection is empty.";
                }
                return $"Document count: {recordCount} \n Sample data: \n {firstDocument.ToJson()} \n Indexes: {string.Join('\n', indexesData)}";
            }
            catch (Exception ex)
            {
                _log.LogError("Error retrieving structure for collection {collectionName}: {message}", collectionName, ex.Message);
                return "Error retrieving structure.";
            }
        }

        public bool CreateIndex(string collectionName, string indexName, string fieldName)
        {
            try
            {
                var collection = mongoDatabase.GetCollection<BsonDocument>(collectionName);
                var indexKeys = Builders<BsonDocument>.IndexKeys.Ascending(fieldName);
                var indexModel = new CreateIndexModel<BsonDocument>(indexKeys, new CreateIndexOptions { Name = indexName });
                collection.Indexes.CreateOne(indexModel);
                _log.LogInformation("Index {indexName} created successfully on collection {collectionName}.", indexName, collectionName);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError("Error creating index {indexName} on collection {collectionName}: {message}", indexName, collectionName, ex.Message);
                return false;
            }
        }
    }
}
