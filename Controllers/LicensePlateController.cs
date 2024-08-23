using DMRWebScrapper_service.Code;
using DMRWebScrapper_service.Models;
using Microsoft.AspNetCore.Mvc;

namespace DMRWebScrapper_service.Controllers
{
    [ApiController]
    [Route("api")]
    public class LicensePlateController : ControllerBase
    {

        // Logger
        private readonly ILogger<LicensePlateController> _logger;

        // Proxy
        private readonly DMRProxy _proxy;

        public LicensePlateController(ILogger<LicensePlateController> logger, DMRProxy proxy)
		{
			_logger = logger;
            _proxy = proxy;
		}

        // Get car data
        [HttpGet("{nummerplade}")]
        public async Task<IActionResult> Get(string nummerplade)
        {
            try
            {

                // Get the data from the DMRProxy
                Bildata? bildata = await DMRProxy.HentOplysninger(nummerplade, DateTime.Now);

                // Check if null is returned
                if (bildata == null)
                {
                    return NotFound();
                }

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

                // Get the data from the DMRProxy
                BildataMin? bildata = await DMRProxy.HentOplysningerMin(nummerplade);

                // Check if null is returned
                if (bildata == null)
                {
                    return NotFound();
                }

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
