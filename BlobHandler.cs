using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Helios
{
    public static class BlobHandler
    {
        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName,
                                                            AzureStorageConfig _storageConfig)
        {
            // Create a URI to the blob
            Uri blobUri = new Uri("https://" +
                                  _storageConfig.AccountName +
                                  ".blob.core.windows.net/" +
                                  _storageConfig.ImageContainer +
                                  "/" + fileName);

            // Create StorageSharedKeyCredentials object by reading
            // the values from the configuration (appsettings.json)
            StorageSharedKeyCredential storageCredentials =
                new StorageSharedKeyCredential(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create the blob client.
            BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

            // Upload the file
            await blobClient.UploadAsync(fileStream);
            return await Task.FromResult(true);
        }

        public static async Task<string> DownloadFileFromStorage(string fileName,
                                                          AzureStorageConfig _storageConfig)
        {
            // Create a URI to the blob
            Uri blobUri = new Uri("https://" +
                                  _storageConfig.AccountName +
                                  ".blob.core.windows.net/" +
                                  _storageConfig.ImageContainer +
                                  "/" + fileName);

            // Create StorageSharedKeyCredentials object by reading
            // the values from the configuration (appsettings.json)
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
                //BlobDownloadInfo download = await blobClient.DownloadAsync();
                //byte[] result = new byte[download.ContentLength];
                //await download.Content.ReadAsync(result, 0, (int)download.ContentLength);

                //return Encoding.UTF8.GetString(result);
            }
            return string.Empty;
        }
    }
}
