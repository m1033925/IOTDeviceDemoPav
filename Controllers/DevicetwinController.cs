using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System.Text;

namespace MyIOTDevice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DevicetwinController : ControllerBase
    {

        private IConfiguration configuration;
        private static RegistryManager registryManager;

        private static DeviceClient client = null;

        public DevicetwinController(IConfiguration _configuration)
        {
            configuration = _configuration;
            registryManager = RegistryManager.CreateFromConnectionString(this.configuration.GetConnectionString("NxTIoTHubSAP"));
            client = DeviceClient.CreateFromConnectionString(this.configuration.GetConnectionString("DeviceConnectionString"), Microsoft.Azure.Devices.Client.TransportType.Mqtt);
        }

        [HttpPut]
        [Route("AddTags")]
        public async Task<IActionResult> AddTags(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException($"'{nameof(deviceId)}' cannot be empty.", nameof(deviceId));
            }
            try
            {
                var twin = await registryManager.GetTwinAsync(deviceId);
                if (twin == null)
                {
                    throw new Exception("Devicetwin Not found for : " + deviceId);
                }

                var patch =
                            @"{
                                tags: {
                                        location: {
                                        region: 'EastUS',
                                        plant: 'Random3'
                                                }
                                        }
                            }";
                await registryManager.UpdateTwinAsync(deviceId, patch, twin.ETag);
                return Ok("Device - " + deviceId + " tag updated ");
            }
            catch (Exception ex)
            {
                throw new Exception("couldn't AddTag for Device " + deviceId + " due to : " + ex);
            }
        }

        [HttpPut]
        [Route("Properties/Desired")]
        public async Task<IActionResult> UpdateDesiredProperty(string deviceId)
        {
            if (deviceId == null)
            {
                throw new ArgumentException($"'{nameof(deviceId)}', cannot be null or empty", nameof(deviceId));
            }
            try
            {
                var twin = await registryManager.GetTwinAsync(deviceId);
                if (twin == null)
                {
                    throw new Exception("Devicetwin Not found for : " + deviceId);
                }
                var patch =
                @"{
                    properties:{
                        desired:{
                            tempLevel: 10
                        }
                    }
                }";
                var result = await registryManager.UpdateTwinAsync(deviceId, patch, twin.ETag);
                return Ok("Device - " + deviceId + "desired property updated");
            }
            catch (System.Exception ex)
            {
                throw new Exception("couldn't update desired property value of deviceid " + deviceId + "due to " + ex.Message);
            }
        }

        [HttpPut]
        [Route("properties/Reported")]
        public async Task<IActionResult> UpdateReportedProperty(string deviceId)
        {
            if (deviceId == null)
            {
                throw new ArgumentException($"'{nameof(deviceId)}', cannot be null or empty", nameof(deviceId));
            }
            try
            {
                TwinCollection reportedProperties, connectivity;
                reportedProperties = new TwinCollection();
                connectivity = new TwinCollection();
                connectivity["type"] = "cellular";
                reportedProperties["connectivity"] = connectivity;
                reportedProperties["coolerOn"] = 0;
                await client.UpdateReportedPropertiesAsync(reportedProperties);
                return Ok("Device - " + deviceId + "reported property updated");
            }
            catch (System.Exception ex)
            {
                throw new Exception("couldn't update desired property value of deviceid " + deviceId + "due to " + ex.Message);
            }
        }

        [HttpPut]
        [Route("properties/Telemetric")]
        public async Task<IActionResult> SendTelemetricMessage(string deviceId)
        {
            if (deviceId == null)
            {
                throw new ArgumentException($"'{nameof(deviceId)}', cannot be null or empty", nameof(deviceId));
            }
            try
            {
                var device = await client.GetTwinAsync();
                //var twinProperties = new ReportedProperties()
                TwinCollection reportedProperties = device.Properties.Reported;
                var telemetryDataPoint = new
                {
                    coolerOn = 1//reportedProperties["coolerOn"],
                };
                string messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString));
                await client.SendEventAsync(message);
                return Ok("Device - " + deviceId + "telemetric message sent successfully");
            }
            catch (System.Exception ex)
            {
                throw new Exception("couldn't update desired property value of deviceid " + deviceId + "due to " + ex.Message);
            }
        }
    }
}