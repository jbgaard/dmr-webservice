namespace DMRWebScrapper_service.Code
{

    using DMRWebScrapper_service.Models;

	// Proxy cache for DMRWebScrapper
	public class DMRProxyCache
	{

		// This cache should store the proxy information from the DMRWebScrapper, so that fewer requests are made to the DMR API
		// The cache should be able to store the proxy information for a certain amount of time, and then remove it

		// Dictionary to store the proxy information
		private Dictionary<string, CacheBildata> _bildataProxyCache;

		// Constructor
		public DMRProxyCache()
		{
			_bildataProxyCache = new Dictionary<string, CacheBildata>();
		}

		// Add bildata to the cache
		public void AddBildataToCache(string regNr, Bildata bildata)
		{
			// Create new CacheBildata
			CacheBildata cacheBildata = new CacheBildata
			{
				Bildata = bildata,
				TimeStamp = DateTime.Now
			};

			_bildataProxyCache.Add(regNr, cacheBildata);
		}

		// Get bildata from the cache
		public Bildata? GetBildataFromCache(string regNr)
		{
			if (_bildataProxyCache.ContainsKey(regNr))
			{

				// Check if the cache is older than 5 days
				if (_bildataProxyCache[regNr].TimeStamp.AddDays(5) < DateTime.Now)
				{
					_bildataProxyCache.Remove(regNr);
					return null;
				}

				return _bildataProxyCache[regNr]?.Bildata;
			}
			else
			{
				return null;
			}
		}
		
		// Class CacheBildata
		public class CacheBildata
		{
			public Bildata Bildata { get; set; }
			public DateTime TimeStamp { get; set; }
		}
	}
}