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

}
