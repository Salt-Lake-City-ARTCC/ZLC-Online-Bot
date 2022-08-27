using System;
using System.Collections.Generic;
using System.Text;

namespace ZLCBotCore.Models.VatsimJsonData
{
    public class VatsimGeneral
    {
        public int? version { get; set; }
        public int? reload { get; set; }
        public string update { get; set; }
        public DateTime? update_timestamp { get; set; }
        public int? connected_clients { get; set; }
        public int? unique_users { get; set; }
    }
}
