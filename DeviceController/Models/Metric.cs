using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MetricsController.Models
{
    public class Metric
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public ulong DeviceId;
        public double Temperature;
        public string Status = "";
    }
}
