using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace Helios
{
    public class Program
    {
        static int counter;
        static string connectionString;
        public static IConfiguration _configuration;
        static bool OnFileListener = true;
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

            ///////////
 //           _configuration = new ConfigurationBuilder()
 //.AddJsonFile("appsettings.json", true, true)
 //.Build();
 //           //Read the Azure storage details
 //           AzureStorageConfig azureStorageConfig = new AzureStorageConfig() { AccountKey = _configuration.GetSection("AzureStorageConfig")["AccountKey"], AccountName = _configuration.GetSection("AzureStorageConfig")["AccountName"], ImageContainer = _configuration.GetSection("AzureStorageConfig")["ImageContainer"] };

 //           //     Console.WriteLine("Connection String:" + connectionString);
 //           //var fileSystemWatchFolder = Environment.GetEnvironmentVariable("FileSystemWatchFolder");
 //           var fileSystemWatchFolder = @"c:\filewatch";
 //           Init().Wait();
 //           Run(fileSystemWatchFolder);
 //           // Wait until the app unloads or is cancelled
 //           var cts = new CancellationTokenSource();
 //           AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
 //           Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
 //           WhenCancelled(cts.Token).Wait();
        }
        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }
        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing information
        /// </summary>
        static async Task Init()
        {
            Console.WriteLine("[{0:HH:mm:ss ffff}]IoT Hub PMSDeviceFileUploader client initialized.", DateTime.Now);
            try
            {

                OnFileListener = true;

            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Error when reading fileupload listener: {0}", ex);
            }

        }
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static void Run(string path)
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            Console.WriteLine($"Local App Data Directory {dir}");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            // Create a new FileSystemWatcher and set its properties.
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = path;

                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName;

                // Only watch text files.
               // watcher.Filter = "*.txt";

                // Add event handlers.
                watcher.Created += OnChanged;

                // Begin watching.
                watcher.EnableRaisingEvents = true;

                // Wait for the user to quit the program.
                Console.WriteLine("Press 'q' to quit the sample.");
                while (Console.Read() != 'q') ;
            }
        }
        private static void OnChanged(object source, System.IO.FileSystemEventArgs e)
        {
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
            Console.WriteLine($"Device listener status: {OnFileListener}");
            if (OnFileListener)
            {
                SendToBlobAsync(e.FullPath);
            }
        }
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static async void SendToBlobAsync(string fileName)
        {
            Console.WriteLine("Uploading file: {0}", fileName);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            ImageSaveToBlobProcess(fileName);
            watch.Stop();
            Console.WriteLine("Time to upload file: {0}ms\n", watch.ElapsedMilliseconds);
        }

        private static void ImageSaveToBlobProcess(string item)
        {
            // Read the Azure storage details
          AzureStorageConfig azureStorageConfig = new AzureStorageConfig() { AccountKey = _configuration.GetSection("AzureStorageConfig")["AccountKey"], AccountName = _configuration.GetSection("AzureStorageConfig")["AccountName"], ImageContainer = _configuration.GetSection("AzureStorageConfig")["ImageContainer"] };

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
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(c =>
                {
                    c.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    //var optionsBuilder = new DbContextOptionsBuilder<PostgreDbContext>();
                    //optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=007;Database=poc");
                    //services.AddScoped<PostgreDbContext>(s => new PostgreDbContext(optionsBuilder.Options));

                    services.AddHostedService<Worker>();
                });

            return host;
        }
        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)

        //        .ConfigureServices((hostContext, services) =>
        //        {
        //            services.AddHostedService<Worker>();
        //        });
    }
}
