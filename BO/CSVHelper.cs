using CsvHelper;
using CsvHelper.Configuration;
using Helios.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Helios
{
    public class CSVHelper : ICSVHelper
    {
        private readonly ILogger<CSVHelper> _logger;
        private readonly IBlobHandler _blobHandler;

        public CSVHelper(ILogger<CSVHelper> logger, IBlobHandler blobHandler)
        {
            _logger = logger;
            _blobHandler = blobHandler;

        }
        public bool UploadCSVFileToBlob(List<CSVPouchData> pouchDetails, string fileName, AzureStorageConfig azureStorageConfig)
        {
            _logger.LogInformation("Started CSV file creating process {0}", fileName);
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

                blobResponse = _blobHandler.UploadFileToStorage(stream, $"{fileName}.csv", azureStorageConfig).GetAwaiter().GetResult();
                _logger.LogInformation("Created CSV file in blob {0}", fileName);
            }
            return blobResponse;
        }
        public bool UploadJSONFileToBlob(List<DepositDataJSON> pouchDetails, string fileName, AzureStorageConfig azureStorageConfig)
        {
            _logger.LogInformation("Started JSON file creating process {0}", fileName);
            var blobResponse = false;
            var data = JsonConvert.SerializeObject(pouchDetails[0]);
            using (MemoryStream stream = new MemoryStream())
            using (TextWriter textWriter = new StreamWriter(stream))
            {
                textWriter.Write(data);
                textWriter.Flush();
                stream.Position = 0;
                blobResponse = _blobHandler.UploadFileToStorage(stream, $"{fileName}.json", azureStorageConfig).GetAwaiter().GetResult();
                _logger.LogInformation("Created JSON file in blob {0}", fileName);
            }
            return blobResponse;
        }
    }
}
