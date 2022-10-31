using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOTDeviceDemoPav;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MyIOTDevice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IoTDeviceBulkController : ControllerBase
    {
        private static RegistryManager registryManager;
        private IConfiguration configuration;

        private readonly ILogger<IoTDeviceBulkController> _logger;

        public IoTDeviceBulkController(IConfiguration _configuration, ILogger<IoTDeviceBulkController> logger)
        {
            configuration = _configuration;
            registryManager = RegistryManager.CreateFromConnectionString(this.configuration.GetConnectionString("NxTIoTHubSAP"));
             _logger = logger;
        }

        [HttpPost]
        private async Task<IActionResult> AddDeviceAsync(string deviceId)
        {

            if (string.IsNullOrEmpty(deviceId))
            {
            throw new ArgumentException($"'{nameof(deviceId)}' cannot be null or empty.", nameof(deviceId));
            }

                Device device = new Device(deviceId);

            try
            {
                System.Console.WriteLine("Creating Device");
               
                device = await registryManager.AddDeviceAsync(device).ConfigureAwait(false);
            }
            catch(DeviceAlreadyExistsException)
            {
                System.Console.WriteLine("Already Exising Device");
                device = await registryManager.GetDeviceAsync(deviceId);
            }

            System.Console.WriteLine("Generated device key : {0}", device.Authentication.SymmetricKey.PrimaryKey);
            return Created("Device Generated with device key: {0}" + device.Authentication.SymmetricKey.PrimaryKey, device);
        }

         [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
            })
            .ToArray();
        }
   } 
}