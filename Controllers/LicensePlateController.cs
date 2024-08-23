using DMRWebScrapper_service.Code;
using DMRWebScrapper_service.Models;
using Microsoft.AspNetCore.Mvc;

namespace DMRWebScrapper_service.Controllers
{
    [ApiController]
    [Route("api")]
    public class LicensePlateController : ControllerBase
    {


        private readonly ILogger<LicensePlateController> _logger;

        public LicensePlateController(ILogger<LicensePlateController> logger)
        {
            _logger = logger;
        }

        // Class for BildataCaching
        class BildataCaching
        {
            public Bildata Bildata { get; set; }
            public DateTime LastUpdated { get; set; }
        }

        // Class for BildataMinCaching
        class BildataMinCaching
        {
            public BildataMin BildataMin { get; set; }
            public DateTime LastUpdated { get; set; }
        }

        // Lists for caching
        private static List<BildataCaching> BildataCache = new List<BildataCaching>();
        private static List<BildataMinCaching> BildataMinCache = new List<BildataMinCaching>();

        [HttpGet("{nummerplade}")]
        public async Task<IActionResult> Get(string nummerplade)
        {
            try
            {
                // Check if the data is in the cache
                BildataCaching? cachedData = BildataCache.FirstOrDefault(x => x.Bildata.Køretøj.Registreringsforhold.RegistreringsNummer == nummerplade);

                // If the data is in the cache and it is not older than 14 days, return the cached data
                if (cachedData != null && cachedData.LastUpdated.AddDays(14) > DateTime.Now)
                {
                    return Ok(cachedData.Bildata);
                }

                // Get the data from the DMRProxy
                Bildata? bildata = await DMRProxy.HentOplysninger(nummerplade, DateTime.Now);

                // Check if null is returned
                if (bildata == null)
                {
                    return NotFound();
                }

                // If the data is not in the cache, add it to the cache
                BildataCaching bildataCaching = new BildataCaching
                {
                    Bildata = bildata,
                    LastUpdated = DateTime.Now
                };

                return Ok(bildata);
            }
            catch
            (Exception ex)
            {
                throw;
            }
        }

        // Minified version of car data
        [HttpGet("minified/{nummerplade}")]
        public async Task<IActionResult> GetMinified(string nummerplade)
        {
            try
            {
                // Check if the data is in the cache
                BildataMinCaching? cachedData = BildataMinCache.FirstOrDefault(x => x.BildataMin.Køretøj.Registreringsforhold.RegistreringsNummer == nummerplade);

                // If the data is in the cache and it is not older than 14 days, return the cached data
                if (cachedData != null && cachedData.LastUpdated.AddDays(14) > DateTime.Now)
                {
                    return Ok(cachedData.BildataMin);
                }

                // Get the data from the DMRProxy
                BildataMin? bildata = await DMRProxy.HentOplysningerMin(nummerplade);

                // Check if null is returned
                if (bildata == null)
                {
                    return NotFound();
                }

                // If the data is not in the cache, add it to the cache
                BildataMinCaching bildataCaching = new BildataMinCaching
                {
                    BildataMin = bildata,
                    LastUpdated = DateTime.Now
                };

                return Ok(bildata);
            }
            catch
            (Exception ex)
            {
                throw;
            }
        }
    }
}
