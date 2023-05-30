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
            await DeferAsync();

            List<string> changedNicknames = new List<string>();
            string error = "";
            string message = "";

            var guildUsers = Context.Guild.GetUsersAsync().Flatten();
            await foreach (var user in guildUsers)
            {
                error = "";
                message = $"{user.Username} ";
                if (user.IsBot)
                {
                    continue;
                }

                SocketGuildUser guildUser = Context.Guild.GetUser(user.Id);
                if (guildUser == null)
                {
                    error = "Unknown Error Occured ";
                    message = "FAILED: " + message + error;
                    changedNicknames.Add(message);
                    _logger.LogWarning(message);
                    continue;
                }

                string oldNikname = user.Nickname ?? "None";
                string newNickname = await _roleService.CreateNickname(guildUser);

                if (oldNikname == newNickname)
                {
                    error = "New Nickname and Old Nickname is the same ";
                    message = "NO CHANGE: " + message + error;
                    _logger.LogDebug(message);
                    continue;
                }

                if (newNickname == string.Empty)
                {
                    error = "Discord Account Not Connected To VATSIM ";
                    message = "FAILED: " + message + error;
                    changedNicknames.Add(message);
                    _logger.LogWarning(message);
                    continue;
                }

                try
                {
                    await guildUser.ModifyAsync(u => u.Nickname = newNickname);
                    message = "SUCCESS: " + message + $"{oldNikname} -> {newNickname} ";
                    changedNicknames.Add(message);
                    _logger.LogInformation(message);
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    message = "FAILED: " + message + error;
                    changedNicknames.Add(message);
                    _logger.LogWarning(message);
                }
            }

            string msg = string.Join("\n", changedNicknames);

            if (msg.Length > 4000)
            {
                _logger.LogWarning("Refresh-Nicknames: Message Response was to large to send to Discord.");
                _logger.LogInformation($"Refresh-nickname: Changed nickname Log:\n\n{msg}");

                msg = "Could not send message due to the length limitation on Discord. If you need a log of the nicknames changed please contact the bot developer (Nikolas).";
            }

            await FollowupAsync(msg);
        }
    }
}
