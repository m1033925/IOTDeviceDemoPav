using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BAl;

namespace Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlobStorageController : ControllerBase
    {
        private readonly IBlobStorage _storage;
        public BlobStorageController(IConfiguration _configuration, IBlobStorage storage)
        {
            _storage = storage;
        }

        [HttpPost("CreateContainer")]
        public async Task<IActionResult> CreateContainerAsync(string containerName)
        {
            BlobContainerClient container;
            try
            {
                container = await _storage.CreateContainerAsync(containerName);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to create Container due to : ", ex);
            }
            return Ok("Container " + containerName + " :" + container + " Created : Successfully");
        }

        [HttpGet("ListAllFiles")]
        public async Task<List<string>> ListAllFiles(string containerName)
        {
            return await _storage.GetAllDocuments(containerName);
        }

        //Upload by using a Stream
        //The following example uploads a blob by creating a Stream object, and then uploading that stream.
        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(string localFilePath, string containerName)
        {
            //string localFilePath=@"C:\Users\M1089675\Documents\TestBlobStorage";
            if (string.IsNullOrEmpty(localFilePath))
            {
                throw new ArgumentException($"'{nameof(localFilePath)}' cannot be null or empty.", nameof(localFilePath));
            }
            string fileName = Path.GetFileName(localFilePath);
            try
            {
                if (!string.IsNullOrEmpty(localFilePath))
                {
                    FileStream fileStream = System.IO.File.Open(localFilePath, FileMode.OpenOrCreate);
                    await _storage.UploadDocument(containerName, fileName, fileStream);
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to Upload file : " + fileName + " due to : " + ex); ;
            }
            return Created("File : " + fileName + " uploaded to Block BLOB storage", "successfully");

        }

        [HttpGet("DownloadFile/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName, string containerName)
        {
            var content = await _storage.GetDocument(containerName, fileName);
            return File(content, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        [HttpDelete("DeleteContainer")]
        public async Task<IActionResult> DeleteContainer(string containerName)
        {
            Azure.Response response;
            try
            {
                response = await _storage.DeleteContainer(containerName);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to Delete Container due to : ", ex);
            }
            return Ok("Container : " + containerName + " Deleted Successfully " + response);
        }

        [HttpDelete("DeleteFile/{fileName}")]
        public async Task<bool> DeleteFile(string fileName, string containerName)
        {
            return await _storage.DeleteDocument(containerName, fileName);
        }
    }
}
