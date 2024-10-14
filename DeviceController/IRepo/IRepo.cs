using MetricsController.Models;

namespace MetricsController.IRepo
{
    public interface IMetricaRepository
    {
        bool Save(Metric metric);
        Metric? Load(ulong deviceId);
        List<Metric> Find(int maxCount);
        long Delete(ulong deviceId);
    }
}
