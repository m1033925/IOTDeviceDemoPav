using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BAl
{
    public class BlobStorage : IBlobStorage
    {
        private readonly string _connectionString;

        public BlobStorage(IConfiguration _configuration)
        {
            _connectionString = _configuration.GetValue<string>("ConnectionStrings:StorageConnectionString");
        }

        //-------------------------------------------------
        // Create a container
        //-------------------------------------------------
        public async Task<BlobContainerClient> CreateContainerAsync(string containerName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            try
            {
                // Create the container
                BlobContainerClient container = await blobServiceClient.CreateBlobContainerAsync(containerName);

                if (await container.ExistsAsync())
                {
                    Console.WriteLine("Created container {0}", container.Name);
                    return container;
                }
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine("HTTP error code {0}: {1}",
                                    e.Status, e.ErrorCode);
                Console.WriteLine(e.Message);
            }

            return null;
        }

        //-------------------------------------------------
        // Create root container
        //-------------------------------------------------
        public void CreateRootContainer()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            try
            {
                // Create the root container or handle the exception if it already exists
                BlobContainerClient container = blobServiceClient.CreateBlobContainer("$root");

                if (container.Exists())
                {
                    Console.WriteLine("Created root container.");
                }
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine("HTTP error code {0}: {1}",
                                    e.Status, e.ErrorCode);
                Console.WriteLine(e.Message);
            }
        }

        public async Task<List<string>> ListContainers(
                                        string containerName,
                                        string prefix,
                                        int? segmentSize)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            List<string> containers = new();
            try
            {
                // Call the listing operation and enumerate the result segment.
                var resultSegment =
                    blobServiceClient.GetBlobContainersAsync(BlobContainerTraits.Metadata, prefix, default)
                    .AsPages(default, segmentSize);

                await foreach (Azure.Page<BlobContainerItem> containerPage in resultSegment)
                {
                    foreach (BlobContainerItem containerItem in containerPage.Values)
                    {
                        containers.Add(containerItem.Name);
                    }

                    return containers;
                }
                return new List<string>();
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        //-------------------------------------------------
        // Delete a container
        //-------------------------------------------------
        public async Task<Azure.Response> DeleteContainer(string containerName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);

            try
            {
                // Delete the specified container and handle the exception.
                Azure.Response response = await container.DeleteAsync();
                return response;
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine("HTTP error code {0}: {1}",
                                    e.Status, e.ErrorCode);
                Console.WriteLine(e.Message);
                throw new Exception(e.Message);
            }
        }

        public BlobContainerClient GetContainer(string containerName)
        {
            BlobServiceClient blobServiceClient = new(_connectionString);
            return blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task UploadDocument(string containerName, string fileName, Stream fileStreamContent)
        {
            var container = GetContainer(containerName);
            //Create a Container If Not exists with BlobServiceClient.BlobServiceClient Represents the Blob Storage endpoint for your storage account.
            if (!await container.ExistsAsync())
            {
                BlobServiceClient blobServiceClient = new(_connectionString);
                await blobServiceClient.CreateBlobContainerAsync(containerName);
                container = blobServiceClient.GetBlobContainerClient(containerName);
            }

            var bobclient = container.GetBlobClient(fileName);
            //The BlobContainerClient.UploadBlobAsync(string, Stream, CancellationToken)operation
            // creates a NEW BLOCK BLOB If not exist. 
            if (!bobclient.Exists())
            {
                fileStreamContent.Position = 0;
                await container.UploadBlobAsync(fileName, fileStreamContent);
            }
            else
            {//For Partial Block-Blob Updates .
                fileStreamContent.Position = 0;
                await bobclient.UploadAsync(fileStreamContent, overwrite: true);
            }
        }

        public async Task<List<string>> GetAllDocuments(string containerName)
        {
            var container = GetContainer(containerName);

            if (!await container.ExistsAsync())
            {
                return new List<string>();
            }

            List<string> blobs = new();

            await foreach (BlobItem blobItem in container.GetBlobsAsync())
            {
                blobs.Add(blobItem.Name);
            }
            return blobs;
        }

        public async Task<Stream> GetDocument(string containerName, string fileName)
        {
            BlobServiceClient blobServiceClient = new(_connectionString);
            var container = GetContainer(containerName);
            if (await container.ExistsAsync())
            {
                var blobClient = container.GetBlobClient(fileName);
                if (blobClient.Exists())
                {
                    var content = await blobClient.DownloadStreamingAsync();
                    return content.Value.Content;
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
            else
            {
                throw new FileNotFoundException();
            }

        }

        public async Task<bool> DeleteDocument(string containerName, string fileName)
        {
            BlobServiceClient blobServiceClient = new(_connectionString);
            var container = GetContainer(containerName);
            if (!await container.ExistsAsync())
            {
                return false;
            }

            var blobClient = container.GetBlobClient(fileName);

            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteIfExistsAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}