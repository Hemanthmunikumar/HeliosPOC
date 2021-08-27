using Azure.Storage.Blobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Helios
{
    public static class TrainedPills
    {
        public static List<Item> TrainedPillItems;
        /// <summary>
        /// 
        /// </summary>
        public static void TrainedPillsProcess()
        {
            ReadDataFromAPI();
            if (TrainedPillItems.Count > 0)
            {
                // read the files
                string App = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                string Templates = Path.Combine(App, "DemoImages");


            }
        }
        private static async void ReadDataFromAPI()
        {

            if (TrainedPillItems == null || TrainedPillItems.Count == 0)
            {
                TrainedPillItems = new List<Item>();
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=automationdevtest;AccountKey=lxYZo1OYcWWwhT5FQ0HTBZZJr5RtilS1jclytHW57bsYKmN9C9xIAPOvMOpDjyFlgi2NZdJtpHmadgLlvd1fKw==;EndpointSuffix=core.windows.net");
                CloudBlockBlob blob = new CloudBlockBlob(new Uri("https://automationdevtest.blob.core.windows.net/alphadata/trained_pills_poc.pbtxt"), storageAccount.Credentials);
                string contents = blob.DownloadTextAsync().Result;
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
