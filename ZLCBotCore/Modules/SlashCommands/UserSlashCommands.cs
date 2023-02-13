using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLCBotCore.Services;

namespace ZLCBotCore.Modules.SlashCommands
{
    public class UserSlashCommands: InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;
        private readonly DiscordSocketClient _discord;
        private readonly ILogger _logger;

        public UserSlashCommands(IServiceProvider services)
        {
            _services = services;
            _config = _services.GetRequiredService<IConfiguration>();
            _discord = _services.GetRequiredService<DiscordSocketClient>(); ;
            _logger = _services.GetRequiredService<ILogger<UserSlashCommands>>();

            _logger.LogDebug("Module: Loaded UserSlashCommands");
        }

        /// <summary>
        /// Slash Command for Giving Roles/Permissions to the user performing the command.
        /// </summary>
        /// <returns>None</returns>
        [SlashCommand("give-role", "Get discord roles/permissions. Your Discord account must be linked on the VATUSA website.")]
        public async Task AssignRoles()
        {
            //await DeferAsync(ephemeral: true); // ephemeral means that only the person doing the command will see the message/response.
            await DeferAsync();
            var embed = await _services.GetRequiredService<RoleAssignmentService>().GiveRole((SocketGuildUser)Context.User);
            await FollowupAsync(embed: embed.Build());
        }
    }
}
