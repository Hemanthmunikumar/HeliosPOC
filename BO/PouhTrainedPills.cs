using Helios.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Helios.BO
{
    public class PouhTrainedPills : IPouchTrainedPills
    {
        private readonly ILogger<PouhTrainedPills> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICSVHelper _cSVHelper;
        private readonly IBlobHandler _blobHandler;
        private readonly List<string> _extensions;
        private static List<Item> _trainedPillItems;
        private string _drugNames;
        private string _folderPath;
        private bool _isCsvFileProcess;
        private bool _isMonoImagesUpload;
        private AzureStorageConfig _azureStorageConfig;
        private AzureStorageConfig _azureStorageImageConfig;
        private string _storageImageFolder;


        public PouhTrainedPills(ILogger<PouhTrainedPills> logger, IConfiguration config, ICSVHelper cSVHelper, IBlobHandler blobHandler)
        {
            _logger = logger;
            _configuration = config;
            _cSVHelper = cSVHelper;
            _blobHandler = blobHandler;
            _extensions = new List<string> { ".jpg" };
        }
        public async Task PillsProcess()
        {
            await Task.Run(() => PouchPillsProcess());

        }

        #region Private Methods
        /// <summary>
        /// Read the onfiguration values
        /// </summary>
        private void ReadConfigValues()
        {
            _azureStorageConfig = new AzureStorageConfig() { AccountKey = _configuration.GetSection("AzureStorageConfig")["AccountKey"], AccountName = _configuration.GetSection("AzureStorageConfig")["AccountName"], ImageContainer = _configuration.GetSection("AzureStorageConfig")["ImageContainer"] };
            _azureStorageImageConfig = new AzureStorageConfig() { AccountKey = _configuration.GetSection("AzureStorageImageConfig")["AccountKey"], AccountName = _configuration.GetSection("AzureStorageImageConfig")["AccountName"], ImageContainer = _configuration.GetSection("AzureStorageImageConfig")["ImageContainer"] };
            _storageImageFolder = $"{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/";
            _folderPath = _configuration.GetSection("HeliosConfig")["FolderPath"];

            bool.TryParse(_configuration.GetSection("HeliosConfig")["IsCsvFileProcess"], out bool csvFlag);
            _isCsvFileProcess = csvFlag;

            bool.TryParse(_configuration.GetSection("HeliosConfig")["IsMonoImagesUpload"], out bool isMonoImageFlag);
            _isMonoImagesUpload = isMonoImageFlag;
        }
        /// <summary>
        /// Read Trained pills from Bolb
        /// </summary>
        private void ReadDataFromAPI()
        {
            _logger.LogInformation("Get the Drug names from Blob");
            if (_trainedPillItems == null || _trainedPillItems.Count == 0)
            {
                _trainedPillItems = new List<Item>();
                string contents = _blobHandler.DownloadFileFromStorage(_configuration.GetSection("HeliosConfig")["TrainedPillsFileName"], _azureStorageConfig).Result;
                _trainedPillItems = JsonConvert.DeserializeObject<List<Item>>(contents);
                _logger.LogInformation("Fethed Drug names from Blob");

            }
        }
        /// <summary>
        /// Get Db connection
        /// </summary>
        /// <returns></returns>
        private NpgsqlConnection GetPGConnection()
        {
            return new NpgsqlConnection(_configuration.GetConnectionString("PGConnection"));
        }
        /// <summary>
        /// Get Pouch Ids
        /// </summary>
        /// <param name="drugnames"></param>
        /// <param name="batchid"></param>
        /// <returns></returns>
        private List<PouchVM> GetPouchs(string imageBatches)
        {
            List<PouchVM> pouchs = new List<PouchVM>();
            using (var npgsqlConnection = GetPGConnection())
            {
                npgsqlConnection.Open();

                Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand("get_allpouchsbybatches", npgsqlConnection);
                cmd.Parameters.AddWithValue(new NpgsqlParameter("p_drugnames", NpgsqlDbType.Varchar)).Value = _drugNames;// "123456789";
                cmd.Parameters.AddWithValue(new NpgsqlParameter("p_batches", NpgsqlDbType.Varchar)).Value = imageBatches;// "123456789";
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
        /// Get JSON file data
        /// </summary>
        /// <param name="pouchId"></param>
        /// <param name="batchId"></param>
        /// <returns></returns>
        private List<DepositDataJSON> GetDepositDataJSONDetails(string pouchId, int? batchId)
        {
            List<DepositDataJSON> pouchDetails = new List<DepositDataJSON>();
            using (var npgsqlConnection = GetPGConnection())
            {
                npgsqlConnection.Open();

                Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand("get_pouch_details", npgsqlConnection);
                cmd.Parameters.AddWithValue(new NpgsqlParameter("p_pouchid", NpgsqlDbType.Varchar)).Value = pouchId;// "03407568";
                cmd.Parameters.AddWithValue(new NpgsqlParameter("p_batchid", NpgsqlDbType.Integer)).Value = batchId;//1003;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var pouch = new DepositDataJSON();
                    var id = Convert.ToInt32(reader["R_id"]);
                    pouch.pouch_id = Convert.ToString(reader["R_pouchid"]);
                    pouch.barcode = Convert.ToString(reader["R_pouchid"]);
                    pouch.pouch_number = Convert.ToString(reader["R_tracepacketid"]);
                    pouch.pouch_size = Convert.ToString(reader["R_concat"]);
                    pouch.pouch_packaged_date = Convert.ToString(reader["R_to_char"]);
                    pouch.administration_date = Convert.ToString(reader["R_intakedate"]);
                    pouch.administration_time = Convert.ToString(reader["R_intaketime"]);
                    //TODO Need dynamic below values
                    pouch.patient = new PatientVM() { patient_name = "Helios Test 3", patient_id = 22075, patient_facility = "Perl", patient_facility_code = "Retail" };
                    pouch.drug = GetDrugs(id);

                    pouch.random_deposit = false;
                    pouch.new_drug = true;
                    pouch.human_pass = true;
                    //pouch.packet_situation = "Missing";
                    pouch.order_type = "Multidose";
                    pouch.pouch_type = "Order";
                    //if (reader["R_repaired"] != null)
                    //{ pouch.Repaired = Convert.ToBoolean(reader["R_repaired"]); }
                    //pouch.StringAgg = Convert.ToString(reader["R_string_agg"]);
                    if (reader["R_ok"] != null)
                    { pouch.algo_pass = Convert.ToBoolean(reader["R_ok"]); }
                    //if (reader["R_userok"] != null)
                    //{ pouch.human_pass = Convert.ToBoolean(reader["R_userok"]); }
                    if (!string.IsNullOrEmpty(Convert.ToString(reader["R_situationnew"])))
                    {
                        pouch.packet_situation = Convert.ToInt32(reader["R_situationnew"]);
                    }
                    if (reader["R_randfrac"] != null)
                    {
                        var randfracc = float.Parse(Convert.ToString(reader["R_randfrac"]), CultureInfo.InvariantCulture.NumberFormat);
                    }
                    pouchDetails.Add(pouch);
                }
            }
            return pouchDetails;
        }
        /// <summary>
        /// Get CSV file data
        /// </summary>
        /// <param name="pouchId"></param>
        /// <returns></returns>
        private List<CSVPouchData> GetTypeCSVPouchDetails(int pouchId)
        {
            List<CSVPouchData> pouchDetails = new List<CSVPouchData>();
            try
            {


                using (var npgsqlConnection = GetPGConnection())
                {
                    npgsqlConnection.Open();

                    Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand("get_pouch_details_type_csv", npgsqlConnection);
                    cmd.Parameters.AddWithValue(new NpgsqlParameter("p_pouchid", NpgsqlDbType.Integer)).Value = pouchId;// "03407568";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var pouch = new CSVPouchData();
                        pouch.drugcode = Convert.ToString(reader["drugcode"]);
                        //pouch.filename = Convert.ToString(reader["filename"]);
                        pouch.class_data = Convert.ToString(reader["class"]);
                        if (!string.IsNullOrEmpty(Convert.ToString(reader["xmin"])))
                        {

                            pouch.xmin = Convert.ToInt32(reader["xmin"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(reader["ymin"])))
                        {

                            pouch.ymin = Convert.ToInt32(reader["ymin"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(reader["xmax"])))
                        {

                            pouch.xmax = Convert.ToInt32(reader["xmax"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(reader["ymax"])))
                        {

                            pouch.ymax = Convert.ToInt32(reader["ymax"]);
                        }

                        pouchDetails.Add(pouch);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("CSV data process exeption {0} ", ex.Message);
            }
            return pouchDetails;
        }
        /// <summary>
        /// Get Drug details
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private List<DrugVM> GetDrugs(int id)
        {
            List<DrugVM> drugDetails = new List<DrugVM>();
            using (var npgsqlConnection = GetPGConnection())
            {
                npgsqlConnection.Open();

                Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand("get_drug_details", npgsqlConnection);
                cmd.Parameters.AddWithValue(new NpgsqlParameter("p_pouchid", NpgsqlDbType.Integer)).Value = id;// "03407568";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var drugData = new DrugVM();
                    drugData.drug_code = Convert.ToString(reader["R_drug_code"]);
                    drugData.drug_quantity = Convert.ToString(reader["R_drug_quantity"]);
                    drugData.generic_name = Convert.ToString(reader["R_generic_name"]);
                    drugData.dispenseMethod = "1:15";
                    drugData.commercial_name = "NOT FOR PACKETS";
                    drugData.strength = "0.625MG/5MG";
                    drugData.shape = "oblong";
                    drugData.color = "BLUE";
                    drugData.imprint = "PREMPRO0.625/5";
                    drugData.imprint2 = null;
                    drugDetails.Add(drugData);
                }
            }
            return drugDetails;
        }
        /// <summary>
        /// Images save to blob
        /// </summary>
        /// <param name="item"></param>
        private void ImageSaveToBlobProcess(string item)
        {
            FileInfo fileInfo = new FileInfo(item);
            var fullPouchId = Path.GetFileNameWithoutExtension(item);
            var blobResponse = false;
            var filepath = _storageImageFolder;
            using (var filestream = System.IO.File.OpenRead(item))
            {
                _logger.LogInformation($"Image {fileInfo.Name} is saving to blob");
                // Read the data from database
                blobResponse = _blobHandler.UploadFileToStorage(filestream, $"{filepath}{fileInfo.Name}", _azureStorageImageConfig).GetAwaiter().GetResult();
                _logger.LogInformation($"Image {fileInfo.Name} saved to blob successfully.");
            }
        }

        /// <summary>
        /// Pouch pills process
        /// </summary>
        private void PouchPillsProcess()
        {
            //Read the Azure storage details
            ReadConfigValues();

            _logger.LogInformation("Images process folder path: {0}", _folderPath);
            // Read the Drug names           
            ReadDataFromAPI();

            if (_trainedPillItems.Count > 0)
            {
                _logger.LogInformation("Collected drug names count: {0}", _trainedPillItems.Count);
                _drugNames = string.Join(",", _trainedPillItems.AsEnumerable().Select(r => r.name).ToList());
                if (!Directory.Exists(_folderPath))
                {
                    _logger.LogInformation("Directory not exist: " + _folderPath);
                }

                if (Directory.Exists(_folderPath))
                {
                    _logger.LogInformation("Directory exist: {0}", _folderPath);
                    var di = new DirectoryInfo(_folderPath);

                    try
                    {

                        var fileGroups = (from file in di.EnumerateFiles("*", SearchOption.AllDirectories).Where(q => _extensions.Contains(q.Extension.ToLower()))
                                          let fileName = file.Name.Split(".")[0].Remove(file.Name.Split(".")[0].Length - 1, 1)
                                          let fileFullpath = file.FullName.Split(_configuration.GetSection("HeliosConfig")["DirectorySplit"]) //TODO: Linux format slipt /. If windows change to \\.
                                          let batchId = (fileFullpath.Length > 2) ? $"{ fileFullpath[fileFullpath.Length - 2]}" : string.Empty
                                          let HashSetKey = (fileFullpath.Length > 4) ? $"{fileName}_{fileFullpath[fileFullpath.Length - 2]}_{fileFullpath[fileFullpath.Length - 3]}_{fileFullpath[fileFullpath.Length - 4]}" : string.Empty
                                          select new BathImages { FileName = file.Name.Split(".")[0], FileFullName = file.FullName, HashSetKey = HashSetKey, Fkbatch = batchId })
                                   .GroupBy(x => x.HashSetKey)
                                   .ToDictionary(g => g.Key, g => g.ToList());


                        //Get bathes from folder images
                        var folderImageBatches = string.Join(",", fileGroups.Values.Select(q => q.FirstOrDefault().Fkbatch).Distinct().ToList());
                        // Get DB pouches
                        _logger.LogInformation("Get the Pouchs by drug names");
                        var _dbPouches = GetPouchs(folderImageBatches);
                        if (!_dbPouches.Any())
                        {
                            _logger.LogInformation("There is no pouch images to process.");
                        }
                        foreach (var pouchVM in _dbPouches)
                        {
                            var jsonPouchDetails = new List<DepositDataJSON>();
                            var csvPouchDetails = new List<CSVPouchData>();
                            try
                            {
                                fileGroups.TryGetValue(pouchVM.HastSetKey, out var fileGroup);
                                if (fileGroup != null)
                                {
                                    _logger.LogInformation($"Started Pouch {pouchVM.Pouchid} images to process");

                                    jsonPouchDetails = GetDepositDataJSONDetails(pouchVM.Pouchid, pouchVM.Fkbatch);

                                    if (jsonPouchDetails.Count > 0)
                                    {
                                        _cSVHelper.UploadJSONFileToBlob(jsonPouchDetails, $"{_storageImageFolder}{pouchVM.Pouchid}C", _azureStorageImageConfig);
                                    }
                                    else
                                    {
                                        _logger.LogInformation($"JSON data not found for pouch {pouchVM.Pouchid}");
                                    }
                                    if (_isCsvFileProcess)
                                    {
                                        csvPouchDetails = GetTypeCSVPouchDetails(pouchVM.Id.Value);
                                        if (csvPouchDetails.Count > 0)
                                        {
                                            _cSVHelper.UploadCSVFileToBlob(csvPouchDetails, $"{_storageImageFolder}{pouchVM.Pouchid}C", _azureStorageImageConfig);
                                        }
                                        else
                                        {
                                            _logger.LogInformation($"CSV data not found for pouch {pouchVM.Pouchid}");
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogInformation($"Enable CSV file process flag in config.");
                                    }
                                    foreach (var item in fileGroup)
                                    {
                                        if ((_isMonoImagesUpload && char.ToUpperInvariant(item.FileName.Last()) == char.ToUpperInvariant(char.Parse("M"))) || char.ToUpperInvariant(item.FileName.Last()) != char.ToUpperInvariant(char.Parse("M")))
                                            ImageSaveToBlobProcess(item.FileFullName);
                                    }

                                    _logger.LogInformation($"Pouch {pouchVM.Pouchid} images process completed");


                                }

                            }
                            catch (Exception ex)
                            {
                                _logger.LogError("Error when reading file process: {0}", ex);
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("error {0}", ex);

                    }
                }
                else
                {
                    _logger.LogInformation("Directory not exist: {0}", _folderPath);
                }

            }
            _logger.LogInformation("Process completed successfully.");
        }
        #endregion
    }
}
