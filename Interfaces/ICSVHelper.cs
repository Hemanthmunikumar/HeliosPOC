using System.Collections.Generic;

namespace Helios.Interfaces
{
    public interface ICSVHelper
    {
        bool UploadCSVFileToBlob(List<CSVPouchData> pouchDetails, string fileName, AzureStorageConfig azureStorageConfig);
        bool UploadJSONFileToBlob(List<DepositDataJSON> pouchDetails, string fileName, AzureStorageConfig azureStorageConfig);
    }
}
