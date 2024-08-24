namespace DMRWebScrapper_service.Code
{
    public class CachingService
    {

        // Dictionary to store the cached objects
        private Dictionary<string, CacheObject> _cache;

        // Class
        private class CacheObject
        {
            public object obj;
            public DateTime timeAdded;

            // Constructor
            public CacheObject(object obj)
            {
                this.obj = obj;
                timeAdded = DateTime.Now;
            }
        }

        // Constructor
        public CachingService()
        {
            _cache = new Dictionary<string, CacheObject>();
        }

        // Add object to the cache
        public void AddToCache(string key, object obj)
        {

            // Create new CacheObject
            CacheObject cacheObject = new CacheObject(obj);

            _cache.Add(key, cacheObject);
        }

        // Get object from the cache
        public object? GetFromCache(string key)
        {
            if (_cache.ContainsKey(key))
            {

                // If the object is older than 7 days, remove it from the cache
                if (_cache[key].timeAdded.AddDays(7) < DateTime.Now)
                {
                    _cache.Remove(key);
                    return null;
                }

                return _cache[key].obj;
            }
            else
            {
                return null;
            }
        }

    }
}
