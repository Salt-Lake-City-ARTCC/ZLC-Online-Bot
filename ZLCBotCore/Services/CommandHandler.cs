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
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZLCBotCore.Services
{
    public class CommandHandler
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider services)
        {
            _services = services;
            _logger = _services.GetRequiredService<ILogger<CommandHandler>>();
            _config = _services.GetRequiredService<IConfiguration>();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commands = _services.GetRequiredService<CommandService>();

            _client.MessageReceived += HandleCommand;

            _logger.LogInformation("Loaded: CommandHandler");
        }

        private async Task HandleCommand(SocketMessage parameterMessage)
        {
            var msg = parameterMessage as SocketUserMessage;

            if (msg == null) return;

            if (msg.Source != Discord.MessageSource.User) return;

            int argPos = 0;

            var context = new CommandContext(_client, msg);

            char prefix = Char.Parse(_config.GetSection("prefix").Value);

            if (!msg.HasCharPrefix(prefix, ref argPos)) return;

            var result = await _commands.ExecuteAsync(context, argPos, _services);

            await LogCommandUse(context, result);

            if (!result.IsSuccess)
            {
                if (result.ErrorReason != "Unknown command.")
                {
                    await msg.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");
                }
                else
                {
                    await msg.Author.SendMessageAsync($"**Error: {result.ErrorReason}**\n\t'{msg.Content}'");
                }
            }
        }

        private async Task LogCommandUse(CommandContext context, IResult result)
        {
            await Task.Run(() =>
            {
                if (context.Channel is IGuildChannel)
                {
                    var logText = $"User: {context.User.Username} ({context.User.Id}) Discord Server: [{context.Guild.Name}] -> [{context.Message.Content}]";
                    _logger.LogInformation(logText);
                }
                else
                {
                    var logText = $"User: {context.User.Username} ({context.User.Id}) -> [{context.Message.Content}]";
                    _logger.LogInformation(logText);
                }
            });
        }
    }
}
