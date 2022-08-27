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
