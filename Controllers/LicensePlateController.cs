using DMRWebScrapper_service.Code;
using DMRWebScrapper_service.Models;
using Microsoft.AspNetCore.Mvc;

namespace DMRWebScrapper_service.Controllers
{
    [ApiController]
    [Route("api")]
    public class LicensePlateController : ControllerBase
    {

		// Dependency injection of the DMRProxyCache
		private readonly DMRProxyCache DMRProxyCache;

        private readonly ILogger<LicensePlateController> _logger;

        public LicensePlateController(ILogger<LicensePlateController> logger, DMRProxyCache dmrProxyCache)
		{
			_logger = logger;
			DMRProxyCache = dmrProxyCache;
		}

        [HttpGet("{nummerplade}")]
        public async Task<IActionResult> Get(string nummerplade)
        {
            try
            {

				// Check if the number plate is in the cache
				Bildata? bildataCache = DMRProxyCache.GetBildataFromCache(nummerplade);

				// If the number plate is in the cache, return the data from the cache
				if (bildataCache != null)
				{
					return Ok(bildataCache);
				}

				// If the number plate is not in the cache, get the data from the DMRProxy
                Bildata bildata = await DMRProxy.HentOplysninger(nummerplade, DateTime.Now);

				// Add the data to the cache
				DMRProxyCache.AddBildataToCache(nummerplade, bildata);

				// Return the data
                return Ok(bildata);
            }
            catch
            (Exception ex)
            {
                throw ex;
            }
        }
    }
}
