using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DMRWebScrapper_service.Code
{
    public class PoliceReportService
    {
        // MongoClient
        private MongoClient _client;

        // DatabaseName = DMR_Webservice
        // CollectionName = UsageReports
        private string DatabaseName = Environment.GetEnvironmentVariable("USER_POLICE_REPORTS_DB_NAME") ?? "DMR_Webservice";
        private string CollectionName = Environment.GetEnvironmentVariable("USER_POLICE_REPORTS_COLLECTION_NAME") ?? "PoliceReports";

        // Constructor
        public PoliceReportService(MongoClient mongoClient)
        {
            _client = mongoClient;

            // Check if the database exists
            if (!_client.ListDatabaseNames().ToList().Contains(DatabaseName))
            {
                _client.GetDatabase(DatabaseName);
            }

            // Check if the collection exists
            if (!_client.GetDatabase(DatabaseName).ListCollectionNames().ToList().Contains(CollectionName))
            {
                _client.GetDatabase(DatabaseName).CreateCollection(CollectionName);
            }

        }

        // Class
        [BsonIgnoreExtraElements]
        [BsonDiscriminator("UserPoliceReportObject")]
        public class UserPoliceReportObject
        {
            public string regnr { get; set; }
            public DateTime timeAdded { get; set; }
            public string userIdentifier { get; set; }

            // Constructor
            public UserPoliceReportObject(string regnr, string userIdentifier)
            {
                this.regnr = regnr;
                timeAdded = DateTime.Now;
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

        // Add police report
        public void AddUserPoliceReport(string regnr, string userIdentifier)
        {
            // Get the collection
            var collection = _client.GetDatabase(DatabaseName).GetCollection<UserPoliceReportObject>(CollectionName);

            // ToLower
            regnr = regnr.ToLower();
            userIdentifier = userIdentifier.ToLower();

            // Check if the user has already reported the vehicle
            var existingReport = collection.Find(x => x.regnr.ToLower() == regnr && x.userIdentifier.ToLower() == userIdentifier).FirstOrDefault();

            // If the user has already reported the vehicle, return
            if (existingReport != null)
            {
                return;
            }

            // Add the report
            collection.InsertOne(new UserPoliceReportObject(regnr, userIdentifier));
        }

        // Remove police report
        public void RemoveUserPoliceReport(string regnr, string userIdentifier)
        {
            // Get the collection
            var collection = _client.GetDatabase(DatabaseName).GetCollection<UserPoliceReportObject>(CollectionName);

            // ToLower
            regnr = regnr.ToLower();
            userIdentifier = userIdentifier.ToLower();

            // Check if the user has already reported the vehicle
            var existingReport = collection.Find(x => x.regnr.ToLower() == regnr && x.userIdentifier.ToLower() == userIdentifier).FirstOrDefault();

            // If the user has not reported the vehicle, return
            if (existingReport == null)
            {
                return;
            }

            // Remove the report
            collection.DeleteOne(x => x.regnr.ToLower() == regnr && x.userIdentifier.ToLower() == userIdentifier);
        }

        // Get all police reports for a vehicle
        public UserPoliceReportSummary GetUserPoliceReportsForVehicle(string regNr)
        {
            // ToLower
            regNr = regNr.ToLower();
            
            // Get total visits
            var allReports = _client.GetDatabase(DatabaseName).GetCollection<UserPoliceReportObject>(CollectionName).Find(x => x.regnr.ToLower() == regNr).ToList();

            // Get last visit
            var lastReport = allReports.OrderByDescending(x => x.timeAdded).FirstOrDefault();

            // Check if there are any visits
            if (lastReport == null)
            {
                return new UserPoliceReportSummary(regNr, 0, null);
            }

            return new UserPoliceReportSummary(regNr, allReports.Count, lastReport.timeAdded);
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
