using System;
using System.Collections.Generic;
using System.Text;

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
        public string pouch_id { get; set; }
        public string barcode { get; set; }
        public string pouch_number { get; set; }
        public string pouch_size { get; set; }
        public string pouch_type { get; set; }
        public string order_type { get; set; }
        public string pouch_packaged_date { get; set; }
        public string administration_date { get; set; }
        public string administration_time { get; set; }
        public PatientVM patient { get; set; }
        public List<DrugVM> drug { get; set; }
        public bool? random_deposit { get; set; }
        public bool? new_drug { get; set; }
        public bool? algo_pass { get; set; }
        public bool? human_pass { get; set; }
        public int? packet_situation { get; set; }
    }
    public class PatientVM
    {
        public int patient_id { get; set; }
        public string patient_name { get; set; }
        public string patient_facility { get; set; }
        public string patient_facility_code { get; set; }
        public string patient_location { get; set; }
        public string patient_unit { get; set; }
        public string patient_room { get; set; }
        public string patient_bed { get; set; }
    }
    public class DrugVM
    {
        public string drug_code { get; set; }
        public string dispenseMethod { get; set; }
        public string generic_name { get; set; }
        public string commercial_name { get; set; }
        public string drug_quantity { get; set; }
        public string strength { get; set; }
        public string shape { get; set; }
        public string color { get; set; }
        public string imprint { get; set; }
        public string imprint2 { get; set; }
    }
    public class CSVPouchData
    {
        public string relativedirpath { get; set; }
        public string filename { get; set; }
        public string class_data  { get; set; }
        public int xmin { get; set; }
        public int ymin { get; set; }
        public int xmax { get; set; }
        public int ymax { get; set; }
    }
}
