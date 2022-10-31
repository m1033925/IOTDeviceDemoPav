using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System.Threading.Tasks;
using System.IO;

namespace MyIOTDevice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileStorageController : ControllerBase
    {
        private readonly string _connectionString;

        public FileStorageController(IConfiguration _configuration)
        {
            _connectionString = _configuration.GetValue<string>("ConnectionStrings: StorageConnectionString");
        }

        [HttpPost("CreateFileShare")]
        public async Task<IActionResult> CreateFileShareAsync(string fileShareName)
        {
            try
            {
                ShareClient shareClient = new ShareClient(_connectionString, fileShareName);
                await shareClient.CreateIfNotExistsAsync();
            }
            catch (System.Exception ex)
            {
                throw new Exception("Unable to create File share due to : ", ex);
            }
            return Ok(fileShareName + " : created successfully");
        }

        [HttpPost("CreateDirectory")]
        public async Task<IActionResult> CreateDirectoryAsync(string fileShareName, string directoryName)
        {
            try
            {
                ShareClient share = new ShareClient(_connectionString, fileShareName);
                if (await share.ExistsAsync())
                {
                    ShareDirectoryClient directory = share.GetDirectoryClient(directoryName);
                    await directory.CreateIfNotExistsAsync();
                }
                else
                {
                    throw new FileNotFoundException("Unable to find Fileshare : ", fileShareName);
                }
            }
            catch (System.Exception ex)
            {
                throw new Exception("Unable to create Directory due to : ", ex);
            }
            return Ok(directoryName + " : created successfully");
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFileAsync(string fileShareName, string directoryName, string localFilePath)
        {
            try
            {
                //string localFilePath=@"C:\Users\M1089675\Documents\FileStorageTest.txt";
                ShareClient share = new ShareClient(_connectionString, fileShareName);
                string fileName = "";
                if (await share.ExistsAsync())
                {
                    var directory = share.GetDirectoryClient(directoryName);
                    fileName = Path.GetFileName(localFilePath);
                    var file = directory.GetFileClient(fileName);
                    using (FileStream stream = System.IO.File.Open(localFilePath, FileMode.OpenOrCreate))
                    {
                        file.Create(stream.Length);
                        stream.Close();
                        stream.Dispose();
                    }
                }
                return Ok("file * " + fileName + " * uploaded successfully");
            }
            catch (System.Exception ex)
            {
                throw new Exception("Unable to upload file due to : ", ex);
            }
        }

        [HttpGet("DownloadFile")]
        public async Task<IActionResult> GetFileAsync(string fileShareName, string directoryName, string fileName)
        {
            try
            {
                ShareClient share = new ShareClient(_connectionString, fileShareName);
                if (await share.ExistsAsync())
                {
                    ShareDirectoryClient directory = share.GetDirectoryClient(directoryName);
                    if (await directory.ExistsAsync())
                    {
                        var file = directory.GetFileClient(fileName);
                        if (await file.ExistsAsync())
                        {
                            ShareFileDownloadInfo download = await file.DownloadAsync();
                        }
                    }
                }
                return Ok("file downloaded successfully");
            }
            catch (System.Exception ex)
            {
                throw new Exception("Unable to download file due to : ", ex);
            }
        }

        [HttpDelete("DeleteFile")]
        public async Task<IActionResult> DeleteFileAsync(string fileShareName, string directoryName, string fileName)
        {
            try
            {
                ShareClient share = new ShareClient(_connectionString, fileShareName);
                if (await share.ExistsAsync())
                {
                    ShareDirectoryClient directoryClient = share.GetDirectoryClient(directoryName);
                    if (await directoryClient.ExistsAsync())
                    {
                        ShareFileClient file = directoryClient.GetFileClient(fileName);
                        if (await file.ExistsAsync())
                        {
                            await file.DeleteIfExistsAsync();
                        }
                    }
                }
                return Ok("file deleted successfully");
            }
            catch (System.Exception ex)
            {
                throw new Exception("Unable to delete file due to : ", ex);
            }
        }

        [HttpDelete("DeleteDirectory")]
        public async Task<IActionResult> DeleteDirectoryAysnc(string fileShareName, string directoryName)
        {
            try
            {
                // Instantiate a ShareClient which will be used to create and manipulate the file share
                ShareClient share = new ShareClient(_connectionString, fileShareName);
                // Ensure that the share exists.
                if (await share.ExistsAsync())
                {
                    //if fileShare present then 
                    // Get a reference to the sample directory
                    ShareDirectoryClient directory = share.GetDirectoryClient(directoryName);
                    // Create the directory if it doesn't already exist
                    await directory.DeleteAsync();
                }
                else
                {
                    throw new FileNotFoundException("Unable to find Fileshare : ", fileShareName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to Delete Directory due to : ", ex);

            }
            return Ok(directoryName + " : Deleted successfully");
        }

        [HttpDelete("DeleteFileShare")]
        public async Task<IActionResult> deleteFileShareAysnc(string fileShareName)
        {
            try
            {
                // Instantiate a ShareClient which will be used to create and manipulate the file share
                ShareClient share = new ShareClient(_connectionString, fileShareName);
                // Create the share if it doesn't already exist
                await share.DeleteAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to Delete File share due to : ", ex);

            }
            return Ok(fileShareName + " : Deleted successfully");
        }
    }
}