using System;
using System.Collections.Generic;
using System.Text;

namespace ZLCBotCore.Models.VatsimJsonData
{
    public class VatsimAtisStation
    {
        public int cid { get; set; }
        public string name { get; set; }
        public string callsign { get; set; }
        public string frequency { get; set; }
        public int? facility { get; set; }
        public int? rating { get; set; }
        public string server { get; set; }
        public int? visual_range { get; set; }
        public string atis_code { get; set; }
        public string[] text_atis { get; set; }
        public DateTime? last_updated { get; set; }
        public DateTime? logon_time { get; set; }
    }
}
