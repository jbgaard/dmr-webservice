using DMRWebScrapper_service.Code;
using DMRWebScrapper_service.Models;
using Microsoft.AspNetCore.Mvc;

namespace DMRWebScrapper_service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LicensePlateController : ControllerBase
    {


        private readonly ILogger<LicensePlateController> _logger;

        public LicensePlateController(ILogger<LicensePlateController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "Getinfo")]

        public async Task<IActionResult> Get(string nummerplade)
        {
            try
            {
                Bildata bildata = await DMRProxy.HentOplysninger(nummerplade, DateTime.Now);
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
