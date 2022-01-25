using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Helios
{
    public static class CSVHelper
    {
        public static bool UploadCSVFileToBlob(List<CSVPouchData> pouchDetails, string fileName, AzureStorageConfig azureStorageConfig)
        {
            Console.WriteLine("Started CSV file creating process {0}", fileName);
            byte[] bin;
            var blobResponse = false;
            CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture);
            using (MemoryStream stream = new MemoryStream())
            using (TextWriter textWriter = new StreamWriter(stream))
            using (CsvWriter csv = new CsvWriter(textWriter, configuration))
            {

                csv.WriteRecords(pouchDetails);
                csv.Flush();
                bin = stream.ToArray();
                stream.Position = 0;

                blobResponse = BlobHandler.UploadFileToStorage(stream, $"{fileName}.csv", azureStorageConfig).GetAwaiter().GetResult();
                Console.WriteLine("Created CSV file in blob {0}", fileName);
            }
            return blobResponse;
        }
        public static bool UploadJSONFileToBlob(List<DepositDataJSON> pouchDetails, string fileName, AzureStorageConfig azureStorageConfig)
        {
            Console.WriteLine("Started JSON file creating process {0}", fileName);
            var blobResponse = false;
            var data = JsonConvert.SerializeObject(pouchDetails[0]);
            using (MemoryStream stream = new MemoryStream())
            using (TextWriter textWriter = new StreamWriter(stream))
            {
                textWriter.Write(data);
                textWriter.Flush();
                stream.Position = 0;
                blobResponse = BlobHandler.UploadFileToStorage(stream, $"{fileName}.json", azureStorageConfig).GetAwaiter().GetResult();
                Console.WriteLine("Created JSON file in blob {0}", fileName);
            }
            return blobResponse;
        }
    }
}
