using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ZLCBotCore.Services
{
    public class StartupService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        public StartupService(IServiceProvider services)
        {
            _services = services;
            _config = _services.GetRequiredService<IConfiguration>();
            _discord = _services.GetRequiredService<DiscordSocketClient>();
            _commands = _services.GetRequiredService<CommandService>();
            _logger = _services.GetRequiredService<ILogger<StartupService>>();

            _discord.Ready += _services.GetRequiredService<VatsimApiService>().Start;

            _logger.LogInformation("Loaded: LoggingService");
        }

        public async Task StartAsync()
        {
            string token = _config.GetRequiredSection("Token").Value;

            if (string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(token))
            {
                _logger.LogError("Token: Discord Token is Missing.");
                throw new Exception("Discord Token is Missing.");
            }

            await _discord.LoginAsync(Discord.TokenType.Bot, token);
            await _discord.StartAsync();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
    }
}
