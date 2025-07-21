using DMRWebScrapper_service.Models;

namespace DMRWebScrapper_service.Code;

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
    
    // Class for returning police check
    public class PoliceCheckReturn
    {
        public bool isPoliceCar { get; set; } = false;
        public bool isUnconfirmedPoliceCar { get; set; } = false;
        public bool isUserReported { get; set; } = false;
        public PoliceReportService.UserPoliceReportSummary userPoliceReportSummary { get; set; }
        public BildataMin bildata { get; set; }

        // Constructor
        public PoliceCheckReturn(bool isPoliceCar, bool isUnconfirmedPoliceCar, bool isUserReported, PoliceReportService.UserPoliceReportSummary userPoliceReportSummary, BildataMin bildata)
        {
            this.isPoliceCar = isPoliceCar;
            this.isUnconfirmedPoliceCar = isUnconfirmedPoliceCar;
            this.isUserReported = isUserReported;
            this.userPoliceReportSummary = userPoliceReportSummary;
            this.bildata = bildata;
        }

    }
}