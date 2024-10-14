using MetricsController.IRepo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Metric = MetricsController.Models.Metric;

namespace MetricsController.Scripts
{
    public class RepositoryLayer : IMetricaRepository
    {
        private MongoClient _mongoClient;
        private IMongoDatabase _database;
        private IMongoCollection<metric> _metricsTable;

        private static string _dataBaseHostName = "127.0.0.1";
        private static string _dataBasePort = "27020";
        private static string _replicaSet = "";
        private static string _databaseName = "somedb";
        private static string _databaseCollection = "Metrics";
        private static RepositoryLayer? _instance;

        public static RepositoryLayer Instance => _instance ??= new RepositoryLayer();

        private RepositoryLayer()
        {
            //mongodb://127.0.0.1:27020,127.0.0.1:27021,127.0.0.1:27022/?replicaSet=shard1&serverSelectionTimeoutMS=2000
            //mongodb://127.0.0.1:27020/?directConnection=true&serverSelectionTimeoutMS=2000&appName=mongosh+2.3.0
            var connectionString = $"mongodb://{_dataBaseHostName}:{_dataBasePort}/?" +
                                            $"{(string.IsNullOrEmpty(_replicaSet) ? "directConnection=true" : $"replicaSet={_replicaSet}")}&" +
                                            $"appName=mongosh+2.3.0&serverSelectionTimeoutMS=5000";
            _mongoClient = new MongoClient(connectionString);
            _database = _mongoClient.GetDatabase(_databaseName);
            _metricsTable = _database.GetCollection<metric>(_databaseCollection);
        }

        public static (string, string, string, string, string) Init(string dataBaseHostName, string dataBasePort, string replicaSet = "", string databaseName = "somedb", string databaseCollection = "Metrics")
        {
            _dataBaseHostName = dataBaseHostName;
            _dataBasePort = dataBasePort;
            _replicaSet = replicaSet;
            _databaseName = databaseName;
            _databaseCollection = databaseCollection;
            _instance = new RepositoryLayer();

            return (_dataBaseHostName, _dataBasePort, _replicaSet, _databaseName, _databaseCollection);
        }

        public bool Save(Metric metric)
        {
            var oldMetric = Load(metric.DeviceId);

            Insert(oldMetric, metric);

            return true;
        }

        public Metric? Load(ulong deviceId)
        {
            var vars = _metricsTable.Find(i => i.deviceId == deviceId);

            return vars.FirstOrDefault()?.ToMetric();
        }

        public long Delete(ulong deviceId)
        {
            return _metricsTable.DeleteOne(i => i.deviceId == deviceId).DeletedCount;
        }
        
        private void Insert(Metric? oldMetric, Metric newMetric)
        {
            if (oldMetric == null)
            {
                _metricsTable.InsertOne(new metric(newMetric));
            }
            else
            {
                newMetric.Id = oldMetric.Id;
                _metricsTable.ReplaceOne(i => i.deviceId == newMetric.DeviceId, new metric(newMetric));
            }
        }

        public List<Metric> Find(int count)
        {
            var vars = _metricsTable.Find("{}").Limit(count).ToList();

            return vars.Select(i => i.ToMetric()).ToList();
        }
        
        private class metric
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id;
            public ulong deviceId;
            public double temperature;
            public string status = "";

            public metric(Metric input)
            {
                Id = input.Id;
                deviceId = input.DeviceId;
                temperature = input.Temperature;
                status = input.Status;
            }

            public Metric ToMetric()
            {
                return new Metric() { Id = Id, DeviceId = deviceId, Temperature = temperature, Status = status };
            }
        }
    }
}
