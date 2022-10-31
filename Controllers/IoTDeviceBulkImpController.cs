using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MyIOTDevice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IoTDeviceBulkImpController : ControllerBase
    {
        private static RegistryManager registryManager;
        private IConfiguration configuration;

        private readonly ILogger<IoTDeviceBulkImpController> _logger;

        public IoTDeviceBulkImpController(IConfiguration _configuration, ILogger<IoTDeviceBulkImpController> logger)
        {
            configuration = _configuration;
            registryManager = RegistryManager.CreateFromConnectionString(this.configuration.GetConnectionString("NxTIoTHubSAP"));
             _logger = logger;
        }

        [HttpPost]
        private async static Task AddDeviceAsync(string deviceId){
            Device device;
            try
            {
                System.Console.WriteLine("Creating Device");
                var dev = new Device(deviceId);
                device = await registryManager.AddDeviceAsync(dev).ConfigureAwait(false);
            }
            catch(DeviceAlreadyExistsException)
            {
                System.Console.WriteLine("Already Exising Device");
                device = await registryManager.GetDeviceAsync(deviceId);
            }

            System.Console.WriteLine("Generated device key : {0}", device.Authentication.SymmetricKey.PrimaryKey);
        }
   } 
}