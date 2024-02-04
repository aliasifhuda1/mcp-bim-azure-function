using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace mcp_bim_azure_function.Business
{
    public class Material
    {
        public string AssetName { get; set; }
        public double Weight { get; set; }
        public string Unit { get; set; }
    }

    public class BlobService
    {
        private readonly IConfigurationService _configurationService;
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            var _storageAccount = _configurationService.GetSecret("storageAccount");
            var _storageKey = _configurationService.GetSecret("storageKey");

            var credential = new StorageSharedKeyCredential(_storageAccount, _storageKey);

            var blobUri = $"https://{_storageAccount}.blob.core.windows.net";
            _blobServiceClient = new BlobServiceClient(new Uri(blobUri), credential);
        }

        /**
         * This function lists all the blob containers in the storage account
         * @return A list of blob containers
         **/
        public async Task ListBlobContainersAsync() 
        {
            var containers = _blobServiceClient.GetBlobContainersAsync();

            Console.WriteLine("Containers are : " + containers);
            await foreach (var container in containers)
            {
                Console.WriteLine(container.Name);
            }
        }

        /**
         * This function uploads fixed data to a blob container
         * @return A list of URIs of the uploaded blobs
         **/
        public async Task<List<Uri>> UploadFixedDataAsync()
        {
            var materials = new List<Material>
            {
                new() { AssetName = "Paint - ICEV2.171", Weight = 6.5, Unit = "kg" },
                new() { AssetName = "Aluminium - General - Average - ICEV2.02", Weight = 12.5, Unit = "kg" },
                new() { AssetName = "Asphalt - 4% (bitumen) binder content - ICEV2.14", Weight = 1.925, Unit = "kg" },
                new() { AssetName = "Bricks - General (common brick) - ICEV2.23", Weight = 204, Unit = "kg" },
                new() { AssetName = "Concrete - CEM I Cement - 810 Mpa - ICEV2.121", Weight = 1199, Unit = "kg" },
                new() { AssetName = "Timber - General - ICEV2.239", Weight = 6.77, Unit = "kg" }
            };

            var containerClient = _blobServiceClient.GetBlobContainerClient("test");
            var uris = new List<Uri>();

            foreach (var material in materials)
            {
                var serializedPayload = JsonSerializer.Serialize(material);
                var blobName = $"{material.AssetName.Replace(" ", "_").Replace("-", "_")}.json";
                var blobClient = containerClient.GetBlobClient(blobName);

                await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(serializedPayload)), true);
                uris.Add(blobClient.Uri);
            }

            return uris;
        }

        /**
         * This function lists all the blobs in a container
         * @return A list of blob content
         **/
        public async Task<List<string>> ListAllBlobsContentAsync()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("test");
            var blobsContent = new List<string>();

            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                BlobDownloadInfo download = await blobClient.DownloadAsync();

                using (StreamReader reader = new(download.Content))
                {
                    string content = reader.ReadToEnd();
                    blobsContent.Add(content);
                }
            }

            return blobsContent;
        }


    }
}
