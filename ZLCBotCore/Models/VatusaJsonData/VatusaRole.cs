using System;
using System.Collections.Generic;
using System.Text;

namespace ZLCBotCore.Models.VatusaJsonData
{
    public class VatusaRole
    {
        public int? id { get; set; }
        public int? cid { get; set; }
        public string facility { get; set; }
        public string role { get; set; }
        public DateTime? created_at { get; set; }
    }
}
