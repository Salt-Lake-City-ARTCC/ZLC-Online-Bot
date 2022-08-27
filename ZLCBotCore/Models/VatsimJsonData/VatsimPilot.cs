using System;
using System.Collections.Generic;
using System.Text;

namespace ZLCBotCore.Models.VatsimJsonData
{
    public class VatsimPilot
    {
        public int? cid { get; set; }
        public string name { get; set; }
        public string callsign { get; set; }
        public string server { get; set; }
        public int? pilot_rating { get; set; }
        public float? latitude { get; set; }
        public float? longitude { get; set; }
        public int? altitude { get; set; }
        public int? groundspeed { get; set; }
        public string transponder { get; set; }
        public int? heading { get; set; }
        public float? qnh_i_hg { get; set; }
        public int? qnh_mb { get; set; }
        public VatsimFlightPlan flight_plan { get; set; }
        public DateTime? logon_time { get; set; }
        public DateTime? last_updated { get; set; }
    }
}
