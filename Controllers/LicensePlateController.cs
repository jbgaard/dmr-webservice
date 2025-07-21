using Azure.Core;
using DMRWebScrapper_service.Authentication;
using DMRWebScrapper_service.Code;
using DMRWebScrapper_service.Models;
using Microsoft.AspNetCore.Mvc;
using static DMRWebScrapper_service.Code.PoliceReportService;

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

        // User police report service
        private readonly PoliceReportService _policeReportService;

        // ADMIN API KEY, if this is defined, all actions are allowed
        private readonly string ADMIN_API_KEY = Environment.GetEnvironmentVariable("ADMIN_API_KEY") ?? "MyVerySecretApiKey";

        // Tools
        private readonly Tools _tools;

        public LicensePlateController(ILogger<LicensePlateController> logger, DMRProxy proxy, VehicleViewService vehicleViewService, PoliceReportService policeReportService, IConfiguration configuration)
        {
            _logger = logger;
            _proxy = proxy;
            _vehicleViewService = vehicleViewService;
            _policeReportService = policeReportService;

            // Get API key from configuration or environment variable with fallback
            ADMIN_API_KEY = configuration.GetValue<string>("AdminApiKey") ?? 
                           Environment.GetEnvironmentVariable("ADMIN_API_KEY") ?? 
                           "MyVerySecretApiKey";

            _tools = new Tools(ADMIN_API_KEY);
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

        // Check if the car is a police car, this will get Car info and check if forsikring = "selvforsikret" and status = "aktiv"
        // Otherwise check for user reports
        [HttpGet("police/{nummerplade}")]
        public async Task<IActionResult> GetPolice(string nummerplade)
        {

            try
            {

                // Create a new police check return object
                Tools.PoliceCheckReturn policeCheckReturn = new Tools.PoliceCheckReturn(false, false, false, null, null);

                // Get the data from the DMRProxy
                BildataMin? bildata = await _proxy.HentOplysningerMin(nummerplade);

                // Check if null is returned
                if (bildata == null)
                {
                    return NotFound();
                }

                // Set bildata
                policeCheckReturn.bildata = bildata;

                // Check if the car is a police car
                if (bildata.Forsikring.Forsikring != null && bildata.Forsikring.Forsikring.Equals("SELVFORSIKRING", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Update police check return object
                    policeCheckReturn.isPoliceCar = true;
                    policeCheckReturn.bildata = bildata;
                }

                // Get user reports
                UserPoliceReportSummary userPoliceReportSummary = _policeReportService.GetUserPoliceReportsForVehicle(nummerplade);

                // If 0 reports, not a police car
                // If between 1 and 5 reports, police car is unconfirmed
                // If more than 5 reports, police car is confirmed
                if (userPoliceReportSummary.totalReports > 0)
                {
                    if (userPoliceReportSummary.totalReports > 5)
                    {
                        // Is confirmed police car
                        policeCheckReturn.isPoliceCar = true;
                        policeCheckReturn.isUserReported = true;
                    }
                    else
                    {
                        // Is unconfirmed police car
                        policeCheckReturn.isUnconfirmedPoliceCar = true;
                    }
                }

                // Set user police report summary
                policeCheckReturn.userPoliceReportSummary = userPoliceReportSummary;

                // Add view to the VehicleViewService
                _vehicleViewService.AddView(nummerplade);

                return Ok(policeCheckReturn);

            }
            catch
            (Exception ex)
            {
                throw;
            }
        }

        // Add police report
        [HttpPost("police/{nummerplade}/report")]
        public async Task<IActionResult> AddPoliceReport(string nummerplade)
        {
            try
            {
                // Get the user identifier
                string? userIdentifier = Request.Headers["User-Identifier"];

                // Check if user identifier is null
                if (userIdentifier == null)
                {
                    return BadRequest("User not identified");
                }

                // Add the police report
                _policeReportService.AddUserPoliceReport(nummerplade, userIdentifier);

                return Ok($"Police report for {nummerplade} has been submitted");
            }
            catch
            (Exception ex)
            {
                throw;
            }
        }

        // Remove police report
        [HttpDelete("police/{nummerplade}/report")]
        public async Task<IActionResult> RemovePoliceReport(string nummerplade)
        {
            try
            {
                // Get the user identifier
                string? userIdentifier = Request.Headers["User-Identifier"];

                // IF ADMIN API KEY is defined, allow all requests
                if (_tools.isAdminKeyAllowedFromRequest(Request) == true)
                {
                    // Get user identifier from query
                    userIdentifier = Request.Query["userIdentifier"];
                }

                // Check if user identifier is null
                if (userIdentifier == null)
                {
                    return BadRequest("User not identified");
                }

                // Remove the police report
                _policeReportService.RemoveUserPoliceReport(nummerplade, userIdentifier);

                // Return success
                return Ok($"Police report for {nummerplade} has been removed");
            }
            catch
            (Exception ex)
            {
                throw;
            }
        }

        // Get all police reports for a vehicle, only admin is allowed to do this
        [HttpGet("police/{nummerplade}/reports")]
        [ApiKey]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserPoliceReportSummary))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPoliceReports(string nummerplade)
        {
            try
            {
                // Get all police reports for a vehicle
                var userPoliceReportSummary = _policeReportService.GetUserPoliceReportsForVehicleByRegNr(nummerplade);

                if (userPoliceReportSummary == null)
                {
                    return NotFound();
                }

                return Ok(userPoliceReportSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting police reports for vehicle {Nummerplade}", nummerplade);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        // Get all police reports for a user_identifier, only admin is allowed to do this
        [HttpGet("police/user/{userIdentifier}")]
        [ApiKey]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPoliceReportsForUser(string userIdentifier)
        {
            try
            {
                // Get all police reports for a user
                var userPoliceReportObjects = _policeReportService.GetUserPoliceReportsForUser(userIdentifier);

                if (userPoliceReportObjects == null || !userPoliceReportObjects.Any())
                {
                    return NotFound();
                }

                return Ok(userPoliceReportObjects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting police reports for user {UserIdentifier}", userIdentifier);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

    }

}
