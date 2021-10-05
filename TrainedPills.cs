using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;

namespace Helios
{
    public static class TrainedPills
    {
        #region Variables
        public static IConfiguration _configuration;
        private static List<Item> _trainedPillItems;
        private static string _drugNames;
        private static string _folderPath;
        private static PouchVM _pouchVM = new PouchVM();
        private static List<PouchDetailsVM> _pouchDetailsVM = new List<PouchDetailsVM>();
        //private static Dictionary<string,PouchVM> _dbPouches = new Dictionary<string, PouchVM>();
        private static AzureStorageConfig _azureStorageConfig;
        //private static IServiceScopeFactory _serviceScopeFactory;
        #endregion

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void TrainedPillsProcess(IServiceScopeFactory serviceScopeFactory)
        {

            //_serviceScopeFactory = serviceScopeFactory;
            //using var scope = _serviceScopeFactory.CreateScope();

            //var dbContext = scope.ServiceProvider.GetRequiredService<PostgreDbContext>();


            // Read the configurations
            Console.WriteLine("Read the configurations");
            _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
            //Read the Azure storage details
            _azureStorageConfig = new AzureStorageConfig() { AccountKey = _configuration.GetSection("AzureStorageConfig")["AccountKey"], AccountName = _configuration.GetSection("AzureStorageConfig")["AccountName"], ImageContainer = _configuration.GetSection("AzureStorageConfig")["ImageContainer"] };
            _folderPath = _configuration.GetSection("HeliosConfig")["FolderPath"];
            Console.WriteLine("Images process folder path: {0}", _folderPath);
            // Read the Drug names
            Console.WriteLine("Get the Drug names from Blob");
            ReadDataFromAPI();
            if (_trainedPillItems.Count > 0)
            {
                Console.WriteLine("Collected drug names count: {0}", _trainedPillItems.Count);
                _drugNames = string.Join(",", _trainedPillItems.AsEnumerable().Select(r => r.name).ToList());

                // Read the files from directory
                //string App = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                var extensions = new List<string> { ".jpg" };
                if (Directory.Exists(_folderPath))
                {
                    Console.WriteLine("Directory exist: {0}", _folderPath);
                    var di = new DirectoryInfo(_folderPath);
                    var fileGroups = (from file in di.EnumerateFiles("*", SearchOption.AllDirectories).Where(q => q.Name.Contains("_") && extensions.Contains(q.Extension.ToLower()))
                                      let fileName = file.Name.Split('_')[0]
                                      let fileFullpath = file.FullName.Split('\\')
                                      // key=pouchid_batchid_month_year
                                      let HashSetKey = $"{fileName}_{fileFullpath[fileFullpath.Length - 2]}_{fileFullpath[fileFullpath.Length - 3]}_{fileFullpath[fileFullpath.Length - 4]}"
                                      select new { fileName, file.FullName, HashSetKey })
                                     .GroupBy(x => x.fileName)
                                     .Where(g => g.Count() <= 2)
                                     .ToDictionary(g => g.Key, g => g.ToList());
                    // Get DB pouches
                    Console.WriteLine("Get the Pouchs by drug names");
                    var _dbPouches = GetPouchs().GroupBy(p => p.HastSetKey, StringComparer.OrdinalIgnoreCase)
                                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
                    //Console.WriteLine("Pouchs by drug names");
                    foreach (var fileGroup in fileGroups)
                    {

                        _pouchDetailsVM = new List<PouchDetailsVM>();
                        foreach (var item in fileGroup.Value)
                        {
                            try
                            {
                                _dbPouches.TryGetValue(item.HashSetKey, out var pouchVM);
                                if (pouchVM != null)
                                {
                                    Console.WriteLine("Started Pouch image process {0}", pouchVM.Pouchid);
                                    _pouchVM = pouchVM;
                                    if (_pouchDetailsVM.Count == 0)
                                    {
                                        _pouchDetailsVM = GetPouchDetails();
                                    }
                                    ImageSaveToBlobProcess(item.FullName);
                                    Console.WriteLine("Pouch image process completed {0}", pouchVM.Pouchid);
                                }

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error when reading file process: {0}", ex);
                                continue;
                            }

                        }

                    }

                    //Delete Empty folders
                    di.DeleteEmptyDirs();
                    Console.WriteLine("Deleted empty folders if any.");
                    //string[] allfiles = Directory.GetFiles(mainDirectoryPath, "*.*", SearchOption.AllDirectories);
                    //foreach (var item in allfiles)
                    //{
                    //    ImageSaveToBlobProcess(item, azureStorageConfig);
                    //}
                }
                else
                {
                    Console.WriteLine("Directory not exist: {0}", _folderPath);
                }

            }
            Console.WriteLine("Process completed successfully.");
        }

        #region Private methods
        private static void ImageSaveToBlobProcess(string item)
        {
            //var fileFullpath = item.Split('\\');
            //var batchId = fileFullpath[fileFullpath.Length - 2];
            //var month = fileFullpath[fileFullpath.Length - 3];
            //var year = fileFullpath[fileFullpath.Length - 4];
            FileInfo fileInfo = new FileInfo(item);
            var fullPouchId = Path.GetFileNameWithoutExtension(item);
            var blobResponse = false;
            var filepath = $"{_pouchVM.Pathyear}/{_pouchVM.Pathmonth}/{_pouchVM.Fkbatch}/";
            //
            // var pouchIds = new List<string>();
            //int.TryParse(batchId, out int batchIdNumber);
            //if (_batchId != batchIdNumber)
            //{
            //    _batchId = batchIdNumber;
            //    pouchIds = GetPouchIds(batchIdNumber);
            //}
            //if (pouchIds.Contains(PouchId))
            //{

            //}
            var result = GetPouchDetailsToUploadFilesBlob(_pouchVM.Fkbatch.Value, fullPouchId, filepath, _azureStorageConfig);

            using (var filestream = System.IO.File.OpenRead(item))
            {
                Console.WriteLine("Started Image file creating process {0}", fileInfo.Name);
                // Read the data from database
                blobResponse = BlobHandler.UploadFileToStorage(filestream, $"{filepath}{fileInfo.Name}", _azureStorageConfig).GetAwaiter().GetResult();
                Console.WriteLine("Created Image file in blob {0}", fileInfo.Name);
            }
            // Delete file if response success
            if (blobResponse)
            {
                fileInfo.Delete();
                Console.WriteLine("Image deleted {0}", fileInfo.FullName);
            }

        }
        /// <summary>
        /// Get PouchDetails To Upload csv,json Files Blob
        /// </summary>
        /// <param name="batchId"></param>
        /// <param name="pouchId"></param>
        /// <param name="filepath"></param>
        /// <param name="azureStorageConfig"></param>
        private static bool GetPouchDetailsToUploadFilesBlob(int batchId, string fulPouchId, string filepath, AzureStorageConfig azureStorageConfig)
        {
            var resonse = false;
            if (_pouchDetailsVM.Count() > 0)
            {
                Console.WriteLine("Pouch details found {0}, pouchid is ", _pouchVM.Pouchid);
                resonse = true;
                CSVHelper.UploadCSVFileToBlob(_pouchDetailsVM, $"{filepath}{fulPouchId}", azureStorageConfig);
                CSVHelper.UploadJSONFileToBlob(_pouchDetailsVM, $"{filepath}{fulPouchId}", azureStorageConfig);
                //Console.WriteLine("Pouch details saved to blob {0}", _pouchVM.Pouchid);
            }
            return resonse;
        }
        /// <summary>
        /// Get Db connection
        /// </summary>
        /// <returns></returns>
        private static NpgsqlConnection GetPGConnection()
        {
            return new NpgsqlConnection(_configuration.GetConnectionString("PGConnection"));
        }
        /// <summary>
        /// Get Pouch Ids
        /// </summary>
        /// <param name="drugnames"></param>
        /// <param name="batchid"></param>
        /// <returns></returns>
        private static List<PouchVM> GetPouchs()
        {
            List<PouchVM> pouchs = new List<PouchVM>();
            using (var npgsqlConnection = GetPGConnection())
            {
                npgsqlConnection.Open();

                Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand("get_allpouchsbydrugs", npgsqlConnection);
                cmd.Parameters.AddWithValue(new NpgsqlParameter("p_drugnames", NpgsqlDbType.Varchar)).Value = _drugNames;// "123456789";
                //cmd.Parameters.AddWithValue(new NpgsqlParameter("p_batchid", NpgsqlDbType.Integer)).Value = batchid;// 1003;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                var reader = cmd.ExecuteReader();
                //var pouchId=
                int fieldCount = reader.FieldCount;

                while (reader.Read())
                {
                    var pouch = new PouchVM();
                    pouch.Id = Convert.ToInt32(reader["r_id"]);
                    pouch.Pouchid = Convert.ToString(reader["r_pouchid"]);
                    if (!string.IsNullOrEmpty(Convert.ToString(reader["r_fkbatch"])))
                    {
                        pouch.Fkbatch = Convert.ToInt32(reader["r_fkbatch"]);
                    }
                    if (!string.IsNullOrEmpty(Convert.ToString(reader["r_pathyear"])))
                    {
                        pouch.Pathyear = Convert.ToInt32(reader["r_pathyear"]);
                    }
                    if (!string.IsNullOrEmpty(Convert.ToString(reader["r_pathmonth"])))
                    {
                        pouch.Pathmonth = Convert.ToInt32(reader["r_pathmonth"]);
                    }
                    pouch.SetHashSetKey(pouch.Pouchid, pouch.Fkbatch, pouch.Pathmonth, pouch.Pathyear);
                    pouchs.Add(pouch);
                }
            }
            return pouchs;
        }
        /// <summary>
        /// Get Pouch Details
        /// </summary>
        /// <param name="pouchId"></param>
        /// <param name="batchId"></param>
        /// <returns></returns>
        private static List<PouchDetailsVM> GetPouchDetails()
        {
            List<PouchDetailsVM> pouchDetails = new List<PouchDetailsVM>();
            using (var npgsqlConnection = GetPGConnection())
            {
                npgsqlConnection.Open();

                Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand("get_pouch_details", npgsqlConnection);
                cmd.Parameters.AddWithValue(new NpgsqlParameter("p_pouchid", NpgsqlDbType.Varchar)).Value = _pouchVM.Pouchid;// "03407568";
                cmd.Parameters.AddWithValue(new NpgsqlParameter("p_batchid", NpgsqlDbType.Integer)).Value = _pouchVM.Fkbatch;//1003;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var pouch = new PouchDetailsVM();
                    pouch.Id = Convert.ToInt32(reader["R_id"]);
                    pouch.Pouchid = Convert.ToString(reader["R_pouchid"]);
                    pouch.Tracepacketid = Convert.ToInt32(reader["R_tracepacketid"]);
                    pouch.Concat = Convert.ToString(reader["R_concat"]);
                    pouch.ToChar = Convert.ToString(reader["R_to_char"]);
                    pouch.Intakedate = Convert.ToString(reader["R_intakedate"]);
                    pouch.Intaketime = Convert.ToString(reader["R_intaketime"]);
                    if (reader["R_repaired"] != null)
                    { pouch.Repaired = Convert.ToBoolean(reader["R_repaired"]); }
                    pouch.StringAgg = Convert.ToString(reader["R_string_agg"]);
                    if (reader["R_ok"] != null)
                    { pouch.Ok = Convert.ToBoolean(reader["R_ok"]); }
                    if (!string.IsNullOrEmpty(Convert.ToString(reader["R_situationnew"])))
                    {
                        pouch.Situationnew = Convert.ToInt32(reader["R_situationnew"]);
                    }
                    if (reader["R_randfrac"] != null)
                    {
                        pouch.Randfrac = float.Parse(Convert.ToString(reader["R_randfrac"]), CultureInfo.InvariantCulture.NumberFormat);
                    }
                    pouchDetails.Add(pouch);
                }
            }
            return pouchDetails;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir"></param>
        private static void DeleteEmptyDirs(this DirectoryInfo dir)
        {
            foreach (DirectoryInfo d in dir.GetDirectories())
                d.DeleteEmptyDirs();

            try
            {
                if (_folderPath != dir.FullName)
                {
                    dir.Delete();
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        /// <summary>
        /// Read Trained pills from Bolb
        /// </summary>
        private static async void ReadDataFromAPI()
        {

            if (_trainedPillItems == null || _trainedPillItems.Count == 0)
            {
                _trainedPillItems = new List<Item>();
                string contents = BlobHandler.DownloadFileFromStorage(_configuration.GetSection("HeliosConfig")["TrainedPillsFileName"], _azureStorageConfig).Result;
                _trainedPillItems = JsonConvert.DeserializeObject<List<Item>>(contents);

            }
        }
        #endregion
    }
    public class Item
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
