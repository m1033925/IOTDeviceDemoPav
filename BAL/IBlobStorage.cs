using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using System.Collections.Generic;
using System.IO;

namespace BAl
{
    public interface IBlobStorage
    {
        Task<BlobContainerClient> CreateContainerAsync(string containerName);
        void CreateRootContainer();
        Task<List<string>> ListContainers(string containerName, string prefix, int? segmentSize);
        Task<Azure.Response> DeleteContainer(string containerName);
        Task UploadDocument(string containerName, string fileName, Stream fileStreamContent);
        Task<List<string>> GetAllDocuments(string containerName);
        Task<Stream> GetDocument(string containerName, string fileName);
        Task<bool> DeleteDocument(string containerName, string fileName);

    }
}