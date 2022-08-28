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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using ZLCBotCore.Models.VatsimJsonData;

namespace ZLCBotCore.Data
{
    public class ControllerLists
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        public List<VatsimController> ZLCOnlineControllers { get; set; }
        public List<VatsimController> CurrentPostedControllers { get; set; }


        public ControllerLists(IServiceProvider services)
        {
            _services = services;
            _logger = _services.GetRequiredService<ILogger<ControllerLists>>();

            ZLCOnlineControllers = new List<VatsimController>();
            CurrentPostedControllers = new List<VatsimController>();

            _logger.LogInformation("Loaded: ControllerLists");
        }


    }
}
