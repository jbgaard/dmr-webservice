using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DMRWebScrapper_service.Code
{
    public class VehicleViewService
    {

        // MongoClient
        private MongoClient _client;

        // DatabaseName = DMR_Webservice
        // CollectionName = UsageReports
        private string DatabaseName = Environment.GetEnvironmentVariable("USAGE_REPORTS_DB_NAME") ?? "DMR_Webservice";
        private string CollectionName = Environment.GetEnvironmentVariable("USAGE_REPORTS_COLLECTION_NAME") ?? "UsageReports";

        // Constructor
        public VehicleViewService(MongoClient mongoClient)
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
        [BsonDiscriminator("VehicleViewObject")]
        public class VehicleViewObject
        {
            public string regnr;
            public DateTime timeAdded;

            // Constructor
            public VehicleViewObject(string regnr)
            {
                this.regnr = regnr;
                timeAdded = DateTime.Now;
            }
        }

        // Class for returning UsageReports, show total amount of visits and last visit
        public class VehicleViewReport
        {
            public string regnr { get; set;}
            public int totalVisits { get; set; } = 0;
            public DateTime? lastVisit { get; set; }

            // Constructor
            public VehicleViewReport(string regnr, int totalVisits, DateTime? lastVisit)
            {
                this.regnr = regnr;
                this.totalVisits = totalVisits != 0 ? totalVisits : this.totalVisits;
                this.lastVisit = lastVisit;
            }
        }

        // Add UsageReport
        public void AddView(string key)
        {
            var collection = _client.GetDatabase(DatabaseName).GetCollection<VehicleViewObject>(CollectionName);
            collection.InsertOne(new VehicleViewObject(key));
        }

        // Get all UsageReports
        public VehicleViewReport GetUsageReportsForVehicle(string regNr)
        {
            // Get total visits
            var allVisits = _client.GetDatabase(DatabaseName).GetCollection<VehicleViewObject>(CollectionName).Find(x => x.regnr == regNr).ToList();

            // Get last visit
            var lastVisit = allVisits.OrderByDescending(x => x.timeAdded).FirstOrDefault();

            // Check if there are any visits
            if (lastVisit == null)
            {
                return new VehicleViewReport(regNr, 0, null);
            }

            return new VehicleViewReport(regNr, allVisits.Count, lastVisit.timeAdded);
        }

    }
}
