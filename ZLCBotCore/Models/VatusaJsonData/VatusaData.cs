/*    
 *    ZLC - Online - Bot, a Discord Bot for automated messages to a Discord channel stating who is online and 
 *    controlling on the VATSIM network.
 *    Copyright (C) 2022 Nikolas Boling(Nikolai558)
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <https://github.com/Salt-Lake-City-ARTCC/ZLC-Online-Bot> or <https://www.gnu.org/licenses/>.
 *
 *    ZLC - Online - Bot  Copyright(C) 2022  Nikolas Boling(Nikolai558)
 *    This program comes with ABSOLUTELY NO WARRANTY.
 *    This is free software, and you are welcome to redistribute it
 *    under certain conditions. 
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace ZLCBotCore.Models.VatusaJsonData
{
    public class VatusaData
    {
        public int? cid { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public object email { get; set; }
        public string facility { get; set; }
        public int? rating { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public bool? flag_needbasic { get; set; }
        public bool? flag_xferOverride { get; set; }
        public DateTime? facility_join { get; set; }
        public bool? flag_homecontroller { get; set; }
        public DateTime? lastactivity { get; set; }
        public object flag_broadcastOptedIn { get; set; }
        public object flag_preventStaffAssign { get; set; }
        public bool? promotion_eligible { get; set; }
        public object transfer_eligible { get; set; }
        public VatusaRole[] roles { get; set; }
        public string rating_short { get; set; }
        public object[] visiting_facilities { get; set; }
        public bool? isMentor { get; set; }
        public bool? isSupIns { get; set; }
        public DateTime? last_promotion { get; set; }
    }
}
