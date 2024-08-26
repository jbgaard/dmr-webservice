using Azure.Core;
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
        private readonly string ADMIN_API_KEY = Environment.GetEnvironmentVariable("ADMIN_API_KEY") ?? "QmFuYW7DhmJsZXJPZ1RvbWF0c292cw==";

        // Tools
        private readonly Tools _tools;

        public LicensePlateController(ILogger<LicensePlateController> logger, DMRProxy proxy, VehicleViewService vehicleViewService, PoliceReportService policeReportService)
        {
            _logger = logger;
            _proxy = proxy;
            _vehicleViewService = vehicleViewService;
            _policeReportService = policeReportService;
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
                PoliceCheckReturn policeCheckReturn = new PoliceCheckReturn(false, false, false, null, null);

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
                if (bildata.Forsikring.Forsikring == "SELVFORSIKRING" && bildata.Forsikring.Aktiv)
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
        public async Task<IActionResult> GetPoliceReports(string nummerplade)
        {
            try
            {
                // IF ADMIN API KEY is defined, allow all requests
                if (_tools.isAdminKeyAllowedFromRequest(Request) == false)
                {
                    return Unauthorized();
                }

                // Get all police reports for a vehicle
                var userPoliceReportSummary = _policeReportService.GetUserPoliceReportsForVehicleByRegNr(nummerplade);
                    
                return Ok(userPoliceReportSummary);
            }
            catch
            (Exception ex)
            {
                throw;
            }
        }

        // Get all police reports for a user_identifier, only admin is allowed to do this
        [HttpGet("police/user/{userIdentifier}")]
        public async Task<IActionResult> GetPoliceReportsForUser(string userIdentifier)
        {
            try
            {
                // IF ADMIN API KEY is defined, allow all requests
                if (_tools.isAdminKeyAllowedFromRequest(Request) == false)
                {
                    return Unauthorized();
                }

                // Get all police reports for a user
                var userPoliceReportObjects = _policeReportService.GetUserPoliceReportsForUser(userIdentifier);

                return Ok(userPoliceReportObjects);
            }
            catch
            (Exception ex)
            {
                throw;
            }
        }

    }

    public class Tools
    {

        // API_KEY
        private readonly string ADMIN_API_KEY;

        // Constructor
        public Tools(string _adminApiKey)
        {
            ADMIN_API_KEY = _adminApiKey;
        }

        // Function to check if defined API_KEY is correct
        private bool isAdminKeyAllowed(string key)
        {
            // Check if key is correct
            return key == ADMIN_API_KEY;
        }

        // Function to check if defined API_KEY is correct, from http request
        public bool isAdminKeyAllowedFromRequest(HttpRequest request)
        {

            // Get the API_KEY from the request
            string? apiKey = request.Headers["X-API-KEY"];

            // Check if key is correct
            return isAdminKeyAllowed(apiKey ?? "DISALLOW");

        }
    }

    // Class for returning police check
    public class PoliceCheckReturn
    {
        public bool isPoliceCar { get; set; } = false;
        public bool isUnconfirmedPoliceCar { get; set; } = false;
        public bool isUserReported { get; set; } = false;
        public UserPoliceReportSummary userPoliceReportSummary { get; set; }
        public BildataMin bildata { get; set; }

        // Constructor
        public PoliceCheckReturn(bool isPoliceCar, bool isUnconfirmedPoliceCar, bool isUserReported, UserPoliceReportSummary userPoliceReportSummary, BildataMin bildata)
        {
            this.isPoliceCar = isPoliceCar;
            this.isUnconfirmedPoliceCar = isUnconfirmedPoliceCar;
            this.isUserReported = isUserReported;
            this.userPoliceReportSummary = userPoliceReportSummary;
            this.bildata = bildata;
        }

    }

}
