using System.IO;
using System.Threading.Tasks;

namespace Helios.Interfaces
{
    public interface IBlobHandler
    {
        Task<bool> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig _storageConfig);
        Task<string> DownloadFileFromStorage(string fileName, AzureStorageConfig _storageConfig);
    }
}
