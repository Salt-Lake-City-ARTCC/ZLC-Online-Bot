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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using ZLCBotCore.Data;
using ZLCBotCore.Services;

namespace ZLCBotCore
{
    public class ZLCBot
    {
        private static IConfiguration _config;

        public async Task StartAsync()
        {
            // create Build Configuration
            var _builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
                                                     .AddJsonFile(path: "appsettings.json")
                                                     .AddUserSecrets<Program>();
            _config = _builder.Build();

            if (_config.GetSection("debuging").GetSection("fastChecksDebuging").Value == "true")
            {
                _config.GetSection("serviceCheckLimit").Value = "17000";
                _config.GetSection("newPostLimit").Value = "1";
                _config.GetSection("corectName").Value = "false";
            }

            DiscordSocketConfig discordConfig = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.DirectMessages | GatewayIntents.GuildMessages | GatewayIntents.Guilds,
                AlwaysDownloadUsers = true
            };

            DiscordSocketClient discordClient = new(discordConfig);

            // configure the services 
            var services = new ServiceCollection().AddSingleton(discordClient)
                                                  .AddSingleton(_config)
                                                  .AddSingleton(new CommandService(new CommandServiceConfig { DefaultRunMode = Discord.Commands.RunMode.Async, LogLevel = LogSeverity.Debug, CaseSensitiveCommands = false, ThrowOnError = false }))
                                                  .AddSingleton<StartupService>()
                                                  .AddSingleton<LoggingService>()
                                                  .AddSingleton<CommandHandler>()
                                                  .AddSingleton(new InteractionService(discordClient, new InteractionServiceConfig { LogLevel = LogSeverity.Debug }))
                                                  .AddSingleton<InteractionHandler>()
                                                  .AddSingleton<DescriptionLists>()
                                                  .AddSingleton<VatsimApiService>()
                                                  .AddSingleton<ControllerLists>()
                                                  .AddSingleton<DescriptionLists>()
                                                  .AddSingleton<RoleAssignmentService>()
                                                  .AddSingleton<VatusaApi>();




            // Need to add logging
            ConfigureServices(services);

            // build the services
            var serviceProvider = services.BuildServiceProvider();



            // Instantiate the Logger
            serviceProvider.GetRequiredService<LoggingService>();
            serviceProvider.GetRequiredService<ILogger<ZLCBot>>().LogInformation("LICENSE: ZLC - Online - Bot  Copyright(C) 2022  Nikolas Boling(Nikolai558)\n\nLICENSE: This program comes with ABSOLUTELY NO WARRANTY.\nLICENSE: This is free software, and you are welcome to redistribute it\nLICENSE: under certain conditions.");

            // Load Services and Initialize required services/modules
            serviceProvider.GetRequiredService<CommandHandler>();
            serviceProvider.GetRequiredService<InteractionHandler>();
            serviceProvider.GetRequiredService<RoleAssignmentService>();
            await serviceProvider.GetRequiredService<InteractionHandler>().InitializeAsync();

            // Start the bot
            await serviceProvider.GetRequiredService<StartupService>().StartAsync();

            // Block program till its closed.
            await Task.Delay(-1);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            //Add SeriLog
            services.AddLogging(configure => configure.AddSerilog());
            //Remove default HttpClient logging as it is extremely verbose
            services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

            var logLevel = _config.GetRequiredSection("logLevel").Value;
            var level = Serilog.Events.LogEventLevel.Error;
            if (!string.IsNullOrEmpty(logLevel))
            {
                switch (logLevel.ToLower())
                {
                    case "error": { level = Serilog.Events.LogEventLevel.Error; break; }
                    case "info": { level = Serilog.Events.LogEventLevel.Information; break; }
                    case "debug": { level = Serilog.Events.LogEventLevel.Debug; break; }
                    case "crit": { level = Serilog.Events.LogEventLevel.Fatal; break; }
                    case "warn": { level = Serilog.Events.LogEventLevel.Warning; break; }
                    case "trace": { level = Serilog.Events.LogEventLevel.Debug; break; }
                }
            }
            Log.Logger = new LoggerConfiguration()
                    .WriteTo.File("logs/zlc-bot.log", rollingInterval: RollingInterval.Day)
                    .WriteTo.Console()
                    .MinimumLevel.Is(level)
                    .CreateLogger();
        }
    }
}
