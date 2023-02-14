using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ZLCBotCore.Services
{
    /// <summary>
    /// Interaction handler for Slash commands and other Discord Interactions
    /// </summary>
    public class InteractionHandler
    {
        // Dependency Injection Services Required
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _discord;
        private readonly InteractionService _interactionCommands;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for Discord Interaction Handler
        /// </summary>
        /// <param name="services">Dependency Injection Service Provider</param>
        public InteractionHandler(IServiceProvider services)
        {
            _services = services;
            _discord = _services.GetRequiredService<DiscordSocketClient>();
            _interactionCommands = _services.GetRequiredService<InteractionService>();
            _logger = _services.GetRequiredService<ILogger<InteractionHandler>>();

            // Subscribe to the Discord.Ready event
            //_discord.Ready += InitializeAsync;

            _logger.LogDebug("Loaded: InteractionHandler");
        }

        /// <summary>
        /// Initialize the Interaction handler. This can only be called once the Discord.Ready event is called.
        /// </summary>
        /// <returns>None</returns>
        public async Task InitializeAsync()
        {
            // Add the slash commands
            await _interactionCommands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _discord.InteractionCreated += HandleInteraction;
        }

        /// <summary>
        /// Handle Slash Commands and execute them
        /// </summary>
        /// <param name="arg">Discord Socket Interaction</param>
        /// <returns>None</returns>
        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                var context = new SocketInteractionContext(_discord, arg);
                await _interactionCommands.ExecuteCommandAsync(context, _services);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Interaction Handler: " + ex.Message);
                throw;
            }
        }
    }
}
