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
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZLCBotCore.Services
{
    public class LoggingService
    {
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly InteractionService _interactionCommands;

        public LoggingService(ILogger<LoggingService> logger, DiscordSocketClient discord, CommandService commands, InteractionService interactionCommands)
        {
            _discord = discord;
            _logger = logger;
            _commands = commands;
            _interactionCommands = interactionCommands;

            _discord.Log += OnLogAsync;
            _commands.Log += OnLogAsync;
            _interactionCommands.Log += OnLogAsync;

            _logger.LogInformation("Loaded: LoggingService");
        }

        public Task OnLogAsync(LogMessage msg)
        {
            string logText = $"{msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";

            switch (msg.Severity.ToString())
            {
                case "Critical": { _logger.LogCritical(logText); break; }
                case "Warning": { _logger.LogWarning(logText); break; }
                case "Info": { _logger.LogInformation(logText); break; }
                case "Verbose": { _logger.LogInformation(logText); break; }
                case "Debug": { _logger.LogDebug(logText); break; }
                case "Error": { _logger.LogError(logText); break; }
            }

            return Task.CompletedTask;
        }
    }
}
