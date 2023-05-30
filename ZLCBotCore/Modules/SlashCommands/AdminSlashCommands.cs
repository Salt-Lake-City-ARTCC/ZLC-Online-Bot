using Discord;
using Discord.Interactions;
using Discord.Rest;
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
    [DefaultMemberPermissions(Discord.GuildPermission.ManageMessages | Discord.GuildPermission.ManageNicknames | Discord.GuildPermission.ManageRoles | Discord.GuildPermission.ManageChannels)]
    public class AdminSlashCommands: InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;
        private readonly DiscordSocketClient _discord;
        private readonly ILogger _logger;
        private readonly RoleAssignmentService _roleService;

        public AdminSlashCommands(IServiceProvider services)
        {
            _services = services;
            _config = _services.GetRequiredService<IConfiguration>();
            _discord = _services.GetRequiredService<DiscordSocketClient>(); ;
            _logger = _services.GetRequiredService<ILogger<AdminSlashCommands>>();
            _roleService = services.GetRequiredService<RoleAssignmentService>();

            _logger.LogDebug("Module: Loaded AdminSlashCommands");
        }

        [SlashCommand("refresh-nicknames", "Staff Command - Refresh the Nicknames of all guild members.")]
        public async Task RefreshNickNames()
        {
            _logger.LogWarning("TEST - Refresh command Called.");
            await DeferAsync(ephemeral: true);


            var guildUsers =  Context.Guild.GetUsersAsync().Flatten();
            await foreach (RestGuildUser user in guildUsers)
            {

                SocketGuildUser guildUser = Context.Guild.GetUser(user.Id);
                string oldNikname = user.Nickname ?? "None";


                var embed = await _roleService.GiveRole(guildUser, false);
                string newNikname = guildUser.Nickname ?? "None";
                if (!user.IsBot)
                {
                    _logger.LogWarning($"User : {user.Nickname ?? user.Username}");
                }
                await FollowupAsync(embed: embed.Build());

            }
        }

    }
}
