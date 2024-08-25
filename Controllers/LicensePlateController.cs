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

        // VehicleViewService
        private readonly VehicleViewService _vehicleViewService;

        public LicensePlateController(ILogger<LicensePlateController> logger, DMRProxy proxy, VehicleViewService vehicleViewService)
		{
			_logger = logger;
            _proxy = proxy;
            _vehicleViewService = vehicleViewService;
		}

        // Get car data
        [HttpGet("{nummerplade}")]
        public async Task<IActionResult> Get(string nummerplade)
        {
            try
            {

                // Get the data from the DMRProxy
                Bildata? bildata = await _proxy.HentOplysninger(nummerplade, DateTime.Now);

                // Check if null is returned
                if (bildata == null)
                {
                    return NotFound();
                }

                // Add view to the VehicleViewService
                _vehicleViewService.AddView(nummerplade);

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
                BildataMin? bildata = await _proxy.HentOplysningerMin(nummerplade);

                // Check if null is returned
                if (bildata == null)
                {
                    return NotFound();
                }

                // Add view to the VehicleViewService
                _vehicleViewService.AddView(nummerplade);

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
