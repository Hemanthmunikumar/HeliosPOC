using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Helios
{
    public class PouchDetailsVM
    {

        public int? Id { get; set; }
        public string Pouchid { get; set; }
        public int? Tracepacketid { get; set; }
        public string Concat { get; set; }
        public string ToChar { get; set; }
        public string Intakedate { get; set; }
        public string Intaketime { get; set; }
        public bool? Repaired { get; set; }
        public string StringAgg { get; set; }
        public bool? Ok { get; set; }
        public int? Situationnew { get; set; }
        public float? Randfrac { get; set; }
    }
    public class DepositDataJSON
    {
        //[JsonProperty(PropertyName = "pouch_id")]
        public string Pouch_id { get; set; }
        //[JsonProperty(PropertyName = "barcode")]
        public string Barcode { get; set; }
        public string Pouch_number { get; set; }
        public string Pouch_size { get; set; }
        public string Pouch_type { get; set; }
        public string Order_type { get; set; }
        public string Pouch_packaged_date { get; set; }
        public string Administration_date { get; set; }
        public string Administration_time { get; set; }
        public PatientVM Patient { get; set; }
        public List<DrugVM> Drug { get; set; }
        public bool? Random_deposit { get; set; }
        public bool? New_drug { get; set; }
        public bool? Algo_pass { get; set; }
        public bool? Human_pass { get; set; }
        public int? Packet_situation { get; set; }
    }
    public class PatientVM
    {
        public int Patient_id { get; set; }
        public string Patient_name { get; set; }
        public string Patient_facility { get; set; }
        public string Patient_facility_code { get; set; }
        public string Patient_location { get; set; }
        public string Patient_unit { get; set; }
        public string Patient_room { get; set; }
        public string Patient_bed { get; set; }
    }
    public class DrugVM
    {
        public string Drug_code { get; set; }
        public string DispenseMethod { get; set; }
        public string Generic_name { get; set; }
        public string Commercial_name { get; set; }
        public string Drug_quantity { get; set; }
        public string Strength { get; set; }
        public string Shape { get; set; }
        public string Color { get; set; }
        public string Imprint { get; set; }
        public string Imprint2 { get; set; }
    }
    public class CSVPouchData
    {
        public string Drugcode { get; set; }
        public string Class_data { get; set; }
        public int Xmin { get; set; }
        public int Ymin { get; set; }
        public int Xmax { get; set; }
        public int Ymax { get; set; }
    }
}
