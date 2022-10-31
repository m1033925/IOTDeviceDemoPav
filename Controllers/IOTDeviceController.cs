using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;


namespace MyIOTDevice.Controllers
{
[ApiController]
[Route("[controller]")]
public class IOTDeviceController : ControllerBase
{
    
    private IConfiguration Configuration;
    private static RegistryManager registryManager;

    public IOTDeviceController(IConfiguration _configuration)
    {
        Configuration = _configuration;
        registryManager = RegistryManager.CreateFromConnectionString(this.Configuration.GetConnectionString("NxTIoTHubSAP"));
    }

    [HttpGet]
    public async Task<IActionResult> GetDeviceAsync(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
        {
            throw new ArgumentException($"'{nameof(deviceId)}' cannot be null or whitespace.", nameof(deviceId));
        }
        Device device;
        try
        {
            device = await registryManager.GetDeviceAsync(deviceId);
        }
        catch (Exception ex)
        {
            throw new Exception("couldn't Get Device " + deviceId + " due to : " + ex);
        }
        if (device == null)
        {
            throw new DeviceNotFoundException(deviceId);
        }

        return Ok(device);
    }

    [HttpPost]
    public async Task<IActionResult> AddDeviceAsync(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
        {
            throw new ArgumentException($"'{nameof(deviceId)}' cannot be null or empty.", nameof(deviceId));
        }

        Device device = new Device(deviceId);
        try
        {
            device = await registryManager.AddDeviceAsync(device);
        }
        catch (DeviceAlreadyExistsException)
        {
            device = await registryManager.GetDeviceAsync(deviceId);
            var returnAction = CreatedAtAction("Device Already exists with device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
            //returnAction.StatusCode = StatusCodes.Status409Conflict;
            return returnAction;
        }
        catch (Exception ex)
        {
            throw new Exception("Device Generated fail due to : {0}", ex);
        }

        return Created("Device Generated with device key: {0}" + device.Authentication.SymmetricKey.PrimaryKey, device);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteDeviceAsync(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
        {
            throw new ArgumentException($"'{nameof(deviceId)}' cannot be null or whitespace.", nameof(deviceId));
        }

        try
        {

            await registryManager.RemoveDeviceAsync(deviceId);
        }

        catch (Exception ex)
        {
            throw new Exception("Device Deletion fail due to : " + ex);
        }

        return Ok(deviceId + "Device Deleted successfully");
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDeviceAsync(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
        {
            throw new ArgumentException($"'{nameof(deviceId)}' cannot be null or whitespace.", nameof(deviceId));
        }
        Device device;
        try
        {
            device = await registryManager.GetDeviceAsync(deviceId);
            if (device == null)
            {
                throw new DeviceNotFoundException(deviceId);
            }
            device.StatusReason = "Updated";
            device = await registryManager.UpdateDeviceAsync(device);
        }

        catch (Exception ex)
        {
            throw new Exception("Device Updation fail due to : " + ex);
        }

        return Ok(device.Id + " Updated successfully");
    }

}
}



