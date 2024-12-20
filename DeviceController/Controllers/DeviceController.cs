using Casualbunker.Server.Common;
using MetricsController.Models;
using MetricsController.Scripts;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MetricsController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : Controller
    {
        [HttpPut("InitRepository/{dataBaseHostName}:{port}")]
        public string InitRepository(string dataBaseHostName, string port, string? replicaSet = "", string databaseName = "somedb", string databaseCollection = "Metrics")
        {
            try
            {
                (dataBaseHostName, port, replicaSet, databaseName, databaseCollection) = RepositoryLayer.Init(dataBaseHostName, port, replicaSet, databaseName, databaseCollection);

                return JsonGeneric.ToJson(new List<string>(){$"HostName='{dataBaseHostName}'", $"Port={port}", $"replicaSet='{replicaSet}'", $"Database='{databaseName}'", $"Collection='{databaseCollection}'"});
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        [HttpGet("Ping")]
        public string Ping()
        {
            try
            {
                return JsonGeneric.ToJson($"{Environment.MachineName} || {System.Net.Dns.GetHostName()}");
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        [HttpPut("Device/{deviceId}/Status")]
        public string DeviceStatus(ulong deviceId, string status)
        {
            try
            {
                return JsonGeneric.ToJson(RepositoryLayer.Instance.UpdateDeviceStatus(deviceId, status));
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        [HttpGet("GetMetric/deviceId={deviceId}")]
        public string GetMetric(ulong deviceId)
        {
            try
            {
                return JsonGeneric.ToJson(RepositoryLayer.Instance.Load(deviceId));
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        [HttpGet("SetMetric/deviceId={deviceId}&temperature={temperature}&status={status}")]
        public string SetMetric(ulong deviceId,  double temperature, string status)
        {
            try
            {
                return RepositoryLayer.Instance.Save(new Metric() { DeviceId = deviceId, Temperature = temperature, Status = status}).ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        [HttpGet("FindDevices/maxCount={maxCount}")]
        public string FindDevices(int maxCount = 100)
        {
            try
            {
                var repo = RepositoryLayer.Instance;

                return JsonGeneric.ToJson(repo.Find(maxCount));
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}