using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;

namespace Helios
{
    public static class TrainedPills
    {
        public static List<Item> TrainedPillItems;
        public static IConfiguration _configuration;
        /// <summary>
        /// 
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void TrainedPillsProcess()
        {
            // Read the configurations
            _configuration = new ConfigurationBuilder()
 .AddJsonFile("appsettings.json", true, true)
 .Build();
            //Read the Azure storage details
            AzureStorageConfig azureStorageConfig = new AzureStorageConfig() { AccountKey = "lxYZo1OYcWWwhT5FQ0HTBZZJr5RtilS1jclytHW57bsYKmN9C9xIAPOvMOpDjyFlgi2NZdJtpHmadgLlvd1fKw==", AccountName = "automationdevtest", ImageContainer = "alphadata" };

            ReadDataFromAPI(azureStorageConfig);
            if (TrainedPillItems.Count > 0)
            {


                string trainedPillIds = string.Join(",", TrainedPillItems.AsEnumerable().Select(r => r.id).ToList());

                // Read the files from directory
                //string App = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var mainDirectoryPath = @"c:\demoimages";
                //  string templates = Path.Combine(App, "DemoImages");
                if (Directory.Exists(mainDirectoryPath))
                {
                    string[] allfiles = Directory.GetFiles(mainDirectoryPath, "*.*", SearchOption.AllDirectories);
                    foreach (var item in allfiles)
                    {
                        ImageSaveToBlobProcess(item, azureStorageConfig);
                    }
                }

            }
        }
        private static void ImageSaveToBlobProcess(string item, AzureStorageConfig azureStorageConfig)
        {
            var fileFullpath = item.Split('\\');
            var batchId = fileFullpath[fileFullpath.Length - 2];
            var month = fileFullpath[fileFullpath.Length - 3];
            var year = fileFullpath[fileFullpath.Length - 4];
            var siteName = "test";
            FileInfo fileInfo = new FileInfo(item);
            var PouchId = Path.GetFileNameWithoutExtension(item);
            var blobResponse = false;
            using (var filestream = System.IO.File.OpenRead(item))
            {
                // Read the data from database
                blobResponse = BlobHandler.UploadFileToStorage(filestream, $"{year}/{month}/{batchId}/{fileInfo.Name}", azureStorageConfig).GetAwaiter().GetResult();
            }
            // Delete file if response success
            if (blobResponse)
            {
                fileInfo.Delete();
            }
        }

        private static string GetFilePath(string fullpath)
        {
            var filename = fullpath.Split('\\').LastOrDefault();
            var destinationPath = $"{DateTime.UtcNow.Year}/{DateTime.UtcNow.Month}/{DateTime.UtcNow.Day}/{filename}";
            return destinationPath;
        }
        private static async void ReadDataFromAPI(AzureStorageConfig azureStorageConfig)
        {

            if (TrainedPillItems == null || TrainedPillItems.Count == 0)
            {
                TrainedPillItems = new List<Item>();
                //CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=automationdevtest;AccountKey=lxYZo1OYcWWwhT5FQ0HTBZZJr5RtilS1jclytHW57bsYKmN9C9xIAPOvMOpDjyFlgi2NZdJtpHmadgLlvd1fKw==;EndpointSuffix=core.windows.net");
                //CloudBlockBlob blob = new CloudBlockBlob(new Uri("https://automationdevtest.blob.core.windows.net/alphadata/trained_pills_poc.pbtxt"), storageAccount.Credentials);
                //string contents = blob.DownloadTextAsync().Result;
                string contents = BlobHandler.DownloadFileFromStorage("trained_pills_poc.pbtxt", azureStorageConfig).Result;
                TrainedPillItems = JsonConvert.DeserializeObject<List<Item>>(contents);

            }
        }
    }
    public class Item
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
