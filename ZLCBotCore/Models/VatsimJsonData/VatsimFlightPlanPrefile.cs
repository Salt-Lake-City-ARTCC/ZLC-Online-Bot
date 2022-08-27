using System;
using System.Collections.Generic;
using System.Text;

namespace ZLCBotCore.Models.VatsimJsonData
{
    public class VatsimFlightPlanPrefile
    {
        public int? cid { get; set; }
        public string name { get; set; }
        public string callsign { get; set; }
        public VatsimPrefileFlightPlan flight_plan { get; set; }
        public DateTime? last_updated { get; set; }
    }
}
