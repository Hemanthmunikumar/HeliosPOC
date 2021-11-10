using System;
using System.Collections.Generic;
using System.Text;

namespace Helios
{
    public class PouchVM
    {
        private string _hashSetKey;
        public int? Id { get; set; }
        public string Pouchid { get; set; }
        public int? Fkbatch { get; set; }
        public int? Pathyear { get; set; }
        public int? Pathmonth { get; set; }
        // key=pouchid_batchid_month_year
        public string HastSetKey { get => _hashSetKey; }
        public void SetHashSetKey(string Pouchid)
        {
            _hashSetKey = $"{Pouchid}";
        }
    }

}
