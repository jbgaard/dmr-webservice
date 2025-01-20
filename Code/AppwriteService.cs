using Appwrite;
using Appwrite.Services;
using Appwrite.Models;
using static DMRWebScrapper_service.Code.PoliceReportService;
using System.Collections;

namespace DMRWebScrapper_service.Code
{

    public class AppwriteService
    {

        // Appwrite client
        Client client;
        Users users;
        Databases databases;

        // Variables
        public readonly string APPWRITE_ENDPOINT;
        public readonly string APPWRITE_PROJECT;
        private readonly string APPWRITE_KEY;

        // Database
        public readonly string APPWRITE_DB;

        // Collections
        public string UserPoliceReportCollection = Environment.GetEnvironmentVariable("APPWRITE_COLLECTION_USERPOLICEREPORTS") ?? "UserPoliceReports";

        // Constructor
        public AppwriteService()
        {

            // Check if environment variables are set
            if (Environment.GetEnvironmentVariable("APPWRITE_ENDPOINT") == null)
            {
                throw new Exception("Environment variable APPWRITE_ENDPOINT is not set");
            }

            if (Environment.GetEnvironmentVariable("APPWRITE_PROJECT") == null)
            {
                throw new Exception("Environment variable APPWRITE_PROJECT is not set");
            }

            if (Environment.GetEnvironmentVariable("APPWRITE_KEY") == null)
            {
                throw new Exception("Environment variable APPWRITE_KEY is not set");
            }

            // Database ID
            if (Environment.GetEnvironmentVariable("APPWRITE_DB") == null)
            {
                throw new Exception("Environment variable APPWRITE_KEY is not set");
            }

            // Set variables
            APPWRITE_ENDPOINT = Environment.GetEnvironmentVariable("APPWRITE_ENDPOINT");
            APPWRITE_PROJECT = Environment.GetEnvironmentVariable("APPWRITE_PROJECT");
            APPWRITE_KEY = Environment.GetEnvironmentVariable("APPWRITE_KEY");
            APPWRITE_DB = Environment.GetEnvironmentVariable("APPWRITE_DB");

            // Create client
            client = new Client()
                .SetEndpoint(APPWRITE_ENDPOINT ?? "") // Your API Endpoint
                .SetProject(APPWRITE_PROJECT ?? "") // Your project ID
                .SetKey(APPWRITE_KEY ?? "");

            // Create services
            users = new Users(client);
            databases = new Databases(client);
        }

        public class UserPoliceReportObject
        {
            public string regnr { get; set; }
            public string userIdentifier { get; set; }

            // Constructor
            public UserPoliceReportObject(string regnr, string userIdentifier)
            {
                this.regnr = regnr;
                this.userIdentifier = userIdentifier;
            }
        }

        // Class for returning UsageReports, show total amount of visits and last visit
        public class UserPoliceReportSummary
        {
            public string regnr { get; set; }
            public int totalReports { get; set; } = 0;
            public DateTime? lastReport { get; set; }

            // Constructor
            public UserPoliceReportSummary(string regnr, int totalReports, DateTime? lastReport)
            {
                this.regnr = regnr;
                this.totalReports = totalReports != 0 ? totalReports : this.totalReports;
                this.lastReport = lastReport;
            }
        }

        // Functions
        // Add police report
        public void AddUserPoliceReport(string regnr, string userIdentifier)
        {

            // ToLower
            regnr = regnr.ToLower();
            userIdentifier = userIdentifier.ToLower();

            // Check if the user has already reported the vehicle
            var existingReport = GetUserPoliceReportsForVehicleByRegNr(regnr).Find(x => x.userIdentifier.ToLower() == userIdentifier);

            // If the user has already reported the vehicle, return
            if (existingReport != null)
            {
                return;
            }

            // Add the report
            var newReport = databases.CreateDocument(APPWRITE_DB,UserPoliceReportCollection, ID.Unique(), new UserPoliceReportObject(regnr, userIdentifier));
        }

        // Remove police report
        public void RemoveUserPoliceReport(string regnr, string userIdentifier)
        {
            // ToLower
            regnr = regnr.ToLower();
            userIdentifier = userIdentifier.ToLower();

            // Check if the user has already reported the vehicle
            var existingReport = GetUserPoliceReportsForVehicleByRegNr(regnr).Find(x => x.userIdentifier.ToLower() == userIdentifier);  
            
            // If the user has not reported the vehicle, return
            if (existingReport == null)
            {
                return;
            }

            // Remove the report
            databases.DeleteDocument(APPWRITE_DB, UserPoliceReportCollection, existingReport["$id"].ToString());


        }

        // Get all police reports for a vehicle
        public UserPoliceReportSummary GetUserPoliceReportsForVehicle(string regNr)
        {
            // ToLower
            regNr = regNr.ToLower();

            // Get total visits
            var allReports = databases.ListDocuments(APPWRITE_DB, UserPoliceReportCollection, new List<string> { "registration=regNr" }).Result;

            // Get last visit
            var lastReport = allReports.Documents.OrderByDescending(x => x.CreatedAt).FirstOrDefault();

            // Check if there are any visits
            if (lastReport == null)
            {
                return new UserPoliceReportSummary(regNr, 0, null);
            }

            return new UserPoliceReportSummary(regNr, allReports.Total, lastReport.CreatedAt);
        }

        // Get all police reports for a user
        public List<UserPoliceReportObject> GetUserPoliceReportsForUser(string userIdentifier)
        {

            // Tolower
            userIdentifier = userIdentifier.ToLower();

            // Get total visits
            var allReports = _client.GetDatabase(DatabaseName).GetCollection<UserPoliceReportObject>(CollectionName).Find(x => x.userIdentifier.ToLower() == userIdentifier).ToList();

            return allReports.ToList();
        }

        // Get all police reports for a vehicle by its nnummerplade
        public List<UserPoliceReportObject> GetUserPoliceReportsForVehicleByRegNr(string regNr)
        {

            // TOlower
            regNr = regNr.ToLower();

            // Get total visits
            var allReports = _client.GetDatabase(DatabaseName).GetCollection<UserPoliceReportObject>(CollectionName).Find(x => x.regnr.ToLower() == regNr).ToList();

            return allReports.ToList();
        }

    }
}
