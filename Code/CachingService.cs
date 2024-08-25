using DMRWebScrapper_service.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace DMRWebScrapper_service.Code
{
    public class CachingService
    {

        // MongoDB client
        private MongoClient _client;

        // DatabaseName = DMR_Webservice
        // CollectionName = Cache
        private string DatabaseName = Environment.GetEnvironmentVariable("CACHE_DB_NAME") ?? "DMR_Webservice";
        private string CollectionName = Environment.GetEnvironmentVariable("CACHE_COLLECTION_NAME") ?? "Cache";

        // Class
        [BsonIgnoreExtraElements]
        [BsonDiscriminator("CacheObject")]
        private class CacheObject
        {
            public string Key;
            public object obj;
            public DateTime timeAdded;

            // Constructor
            public CacheObject(string key, object obj)
            {
                Key = key;
                this.obj = obj;
                timeAdded = DateTime.Now;
            }
        }

        // Constructor
        public CachingService(MongoClient mongoClient)
        {
            _client = mongoClient;

            // Check if the database exists
            if (!_client.ListDatabaseNames().ToList().Contains(DatabaseName))
            {
                _client.GetDatabase(DatabaseName);
            }

            // Check if the collection exists
            if (!_client.GetDatabase(DatabaseName).ListCollectionNames().ToList().Contains(CollectionName))
            {
                _client.GetDatabase(DatabaseName).CreateCollection(CollectionName);
            }

            // Adjust serializer to allow CacheObject
            var objectSerializer = new ObjectSerializer(ObjectSerializer.AllAllowedTypes);
            BsonSerializer.RegisterSerializer(objectSerializer);

            // Register classes
            if (!BsonClassMap.IsClassMapRegistered(typeof(Bildata)))
            {
                BsonClassMap.RegisterClassMap<Bildata>();
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(BildataMin)))
            {
                BsonClassMap.RegisterClassMap<BildataMin>();
            }


        }

        // Add object to the cache
        public void AddToCache(string key, object obj)
        {

            // Create new CacheObject
            CacheObject cacheObject = new CacheObject(key, obj);

            // Check if object with the same key already exists
            if (_client.GetDatabase(DatabaseName).GetCollection<CacheObject>(CollectionName).Find(x => x.Key == key).FirstOrDefault() != null)
            {
                _client.GetDatabase(DatabaseName).GetCollection<CacheObject>(CollectionName).DeleteOne(x => x.Key == key);
            }

            // Add object to the cache
            _client.GetDatabase(DatabaseName).GetCollection<CacheObject>(CollectionName).InsertOne(cacheObject);
        }

        // Get object from the cache
        public object? GetFromCache(string key)
        {

            // Get object from the cache
            CacheObject cacheObject = _client.GetDatabase(DatabaseName).GetCollection<CacheObject>(CollectionName).Find(x => x.Key == key).FirstOrDefault();

            // Check if the object exists
            if (cacheObject != null)
            {

                // Check if the object is older than 7 days
                if (DateTime.Now.Subtract(cacheObject.timeAdded).TotalDays > 7)
                {
                    _client.GetDatabase(DatabaseName).GetCollection<CacheObject>(CollectionName).DeleteOne(x => x.Key == key);
                    return null;
                }

                // Return
                return cacheObject.obj;
            }
            else
            {
                return null;
            }
            
        }

    }
}
