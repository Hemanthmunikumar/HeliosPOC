using Azure.Storage;
using Azure.Storage.Blobs;
using Helios.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Helios
{
    public class BlobHandler : IBlobHandler
    {
        public async Task<bool> UploadFileToStorage(Stream fileStream, string fileName,
                                                            AzureStorageConfig _storageConfig)
        {
            // Create a URI to the blob
            Uri blobUri = new Uri("https://" +
                                  _storageConfig.AccountName +
                                  ".blob.core.windows.net/" +
                                  _storageConfig.ImageContainer +
                                  "/" + fileName);

            StorageSharedKeyCredential storageCredentials =
                new StorageSharedKeyCredential(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create the blob client.
            BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

            // Upload the file
            await blobClient.UploadAsync(fileStream, overwrite: true);
            return await Task.FromResult(true);
        }

        public async Task<string> DownloadFileFromStorage(string fileName,
                                                          AzureStorageConfig _storageConfig)
        {
            // Create a URI to the blob
            Uri blobUri = new Uri("https://" +
                                  _storageConfig.AccountName +
                                  ".blob.core.windows.net/" +
                                  _storageConfig.ImageContainer +
                                  "/" + fileName);

            StorageSharedKeyCredential storageCredentials =
                new StorageSharedKeyCredential(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create the blob client.
            BlobClient blobClient = new BlobClient(blobUri, storageCredentials);
            if (await blobClient.ExistsAsync())
            {
                using (var memorystream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(memorystream);
                    byte[] result = memorystream.ToArray();
                    return Encoding.UTF8.GetString(result);
                }
            }
            return string.Empty;
        }
    }
}
