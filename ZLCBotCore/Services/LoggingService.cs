using Discord;
using Discord.Commands;
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

        public LoggingService(ILogger<LoggingService> logger, DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _logger = logger;
            _commands = commands;

            _discord.Log += OnLogAsync;
            _commands.Log += OnLogAsync;

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
