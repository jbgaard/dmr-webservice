namespace DMRWebScrapper_service.Code
{
    public class CachingService
    {

        // Type of object to cache
        public Type Type { get; set; }

        // Dictionary to store the cached objects
        private Dictionary<string, object> _cache;

        // Constructor
        public CachingService(Type type)
        {
            Type = type;
            _cache = new Dictionary<string, object>();
        }

        // Add object to the cache
        public void AddToCache(string key, object obj)
        {
            _cache.Add(key, obj);
        }

        // Get object from the cache
        public object GetFromCache(string key)
        {
            if (_cache.ContainsKey(key))
            {
                return _cache[key];
            }
            else
            {
                return null;
            }
        }

    }
}
