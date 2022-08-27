using System;
using System.Collections.Generic;
using System.Text;

namespace ZLCBotCore.Models.VatsimJsonData
{
    public class VatsimJsonRootModel
    {
        public VatsimGeneral general { get; set; }
        public VatsimPilot[] pilots { get; set; }
        public VatsimController[] controllers { get; set; }
        public VatsimAtisStation[] atis { get; set; }
        public VatsimServer[] servers { get; set; }
        public VatsimFlightPlanPrefile[] prefiles { get; set; }
        public VatsimFacility[] facilities { get; set; }
        public VatsimRating[] ratings { get; set; }
        public VatsimPilotRatings[] pilot_ratings { get; set; }
    }
}
