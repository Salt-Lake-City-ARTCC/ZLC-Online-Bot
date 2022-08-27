using System;
using System.Collections.Generic;
using System.Text;

namespace ZLCBotCore.Models.VatsimJsonData
{
    public class VatsimServer
    {
        public string ident { get; set; }
        public string hostname_or_ip { get; set; }
        public string location { get; set; }
        public string name { get; set; }
        public int? clients_connection_allowed { get; set; }
        public bool client_connections_allowed { get; set; }
        public bool is_sweatbox { get; set; }
    }
}
