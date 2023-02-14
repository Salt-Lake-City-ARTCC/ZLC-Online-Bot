using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLCBotCore.Data;
using static ZLCBotCore.Models.VatusaData.VatusaUserModel;

namespace ZLCBotCore.Services
{
    public class RoleAssignmentService
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _discord;
        private readonly ILogger _logger;
        private readonly VatusaApi _vatusaApi;

        public RoleAssignmentService(IServiceProvider services)
        {
            _services = services;
            _discord = _services.GetRequiredService<DiscordSocketClient>();
            _logger = _services.GetRequiredService<ILogger<RoleAssignmentService>>();
            _vatusaApi = _services.GetRequiredService<VatusaApi>();

            // Handle discord events.
            //_discord.UserJoined += UserJoined;

            _logger.LogDebug("Loaded: RoleAssignmentService");
        }

        /// <summary>
        /// Give specific roles/permissions to users that have their discord account linked to VATUSA
        /// </summary>
        /// <param name="User">Socket Guild User from Discord</param>
        /// <param name="SendDM_OnVatusaNotFound">Send a direct message to the user stating their account's are not linked.</param>
        /// <returns>Embed Builder showing New roles and nickname that was assigned to that user.</returns>
        public async Task<EmbedBuilder> GiveRole(SocketGuildUser User, bool SendDM_OnVatusaNotFound = true)
        {
            EmbedBuilder embed = new()
            {
                Color = Color.Green,
                Title = "Your roles have been assigned"
            };

            VatusaUserData? userModel = await VatusaApi.GetVatusaUserInfo(User.Id);

            string guildName = User.Guild.Name;

            if (userModel == null && SendDM_OnVatusaNotFound)
            {
                SocketGuildChannel rolesChannel = User.Guild.Channels.First(x => x.Name == "role-assignment");

                string linkInstructions =
                    $"Hello, I am an automated program that is here to help you get your `{guildName}` Discord permissions/roles setup.\n\n" +
                    "To do this, I need you to sync your Discord account with the VATUSA Discord server; You may do this by going to your VATUSA profile https://vatusa.net/my/profile > “VATUSA Discord Link”.\n\n" +
                    $"When you are complete, go to the <#{rolesChannel.Id}> discord server and complete the `\\Give Roles` command.\n\n" +
                    "If you are unable to do this, please private message one of the Administrators/Staff Members of the discord.";

                await User.CreateDMChannelAsync().Result.SendMessageAsync(linkInstructions);
                _logger.LogInformation($"No Role: {User.Username} ({User.Id}) in {User.Guild.Name} -> Not found in VATUSA, no roles were assigned.");

                embed.Title = "Not Linked";
                embed.Description = "Your Discord account is not linked on VATUSA. Link it here: \nhttps://vatusa.net/my/profile";
                embed.Color = Color.Red;
                return embed;
            }

            if (userModel == null) return new EmbedBuilder() { Title = "Not Linked", Color = Color.Red, Description = "Your Discord account is not linked on VATUSA. Link it here: \nhttps://vatusa.net/my/profile" };

            List<string> addRoles = DetermineRoles(userModel);

            if (addRoles.Count() > 0)
            {
                foreach (string roleName in addRoles)
                {
                    SocketRole? role = User.Guild.Roles.First(x => x.Name == roleName);
                    if (role == null) continue;

                    await User.AddRoleAsync(role);
                    _logger.LogInformation($"Give Role: {User.Username} ({User.Id}) in {User.Guild.Name} -> Found user in VATUSA; Assigned {role?.Name} role to user.");
                    embed.Description += role?.Mention + " ";
                }
            }

            var nickname = await ChangeNickname(User, userModel);

            embed.Footer = new EmbedFooterBuilder() { Text = "Your new nickname is: " + nickname };

            return embed;
        }

        private List<string> DetermineRoles(VatusaUserData userModel)
        {
            List<string> output = new List<string>();

            if (userModel.data?.facility == "ZLC")
            {
                if (HasArtccStaffRole(userModel)) output.Add("Staff");
                if (IsZLCSpecialist(userModel)) output.Add("ZLC Specialist");
                if (IsZLCObserver(userModel)) output.Add("Observer");
            }

            if (IsZLCVisitor(userModel)) output.Add("ZLC Visitor");

            if (IsZLCNeighbor(userModel)) output.Add("ZLC Neighbor");

            if (output.Count() == 0) output.Add("Guest");

            return output;
        }

        private bool IsZLCObserver(VatusaUserData userModel)
        {
            //        | 0 |   1  |   2 |  3  |  4  |  5  |  6  |  7  |  8  |  9  | 10  |  11  |  12  |
            // Ratings: "", "OBS", "S1", "S2", "S3", "C1", "C2", "C3", "I1", "I2", "I3", "SUP", "ADM"

            if (userModel == null) return false;

            if (new string[] { "OBS" }.Contains(userModel.data?.rating_short))
            {
                return true;
            }
            return false;
        }

        private bool IsZLCVisitor(VatusaUserData userModel)
        {
            if (userModel.data?.visiting_facilities == null) return false;

            foreach (var visitingFacility in userModel.data?.visiting_facilities)
            {
                if (visitingFacility.facility == "ZLC")
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsZLCSpecialist(VatusaUserData userModel)
        {
            //        | 0 |   1  |   2 |  3  |  4  |  5  |  6  |  7  |  8  |  9  | 10  |  11  |  12  |
            // Ratings: "", "OBS", "S1", "S2", "S3", "C1", "C2", "C3", "I1", "I2", "I3", "SUP", "ADM"

            if (userModel == null) return false;

            if (new string[] { "S1", "S2", "S3", "C1", "C2", "C3", "I1", "I2", "I3", "SUP" }.Contains(userModel.data?.rating_short))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Change the Users Nickname. If the user has a "|" in their nickname only change it AFTER the pipe symbol
        /// </summary>
        /// <param name="User">Socket Guild User from discord.</param>
        /// <param name="UserData">User Model from VATUSA API</param>
        /// <returns>A string for what the user's nickname was changed to.</returns>
        private async Task<string> ChangeNickname(SocketGuildUser User, VatusaUserData? UserData)
        {
            string newNickname = $"{UserData?.data?.fname} {UserData?.data?.lname} | {UserData?.data?.rating_short?.ToUpper()} | {UserData?.data?.facility}";

            if (User.Nickname != null && User.Nickname.Contains('|'))
            {
                newNickname = User.Nickname[..User.Nickname.IndexOf("|")] + newNickname[newNickname.IndexOf("|")..];
            }

            try
            {
                _logger.LogInformation($"Nickname: Changing {User.Username} ({User.Id}) nickname -> from {User.Nickname} to {newNickname}");
                await User.ModifyAsync(u => u.Nickname = newNickname);
                return newNickname;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Permissions"))
                {
                    _logger.LogWarning($"Missing Permissions: Could not change Nickname for {User.Username} ({User.Id}) in {User.Guild.Name}");
                    return "I could not change your nickname.";
                }
                throw;
            }
        }

        /// <summary>
        /// Check for any staff roles in the User Model from VATUSA API
        /// </summary>
        /// <param name="userData">User Modle from VATUSA API</param>
        /// <returns>True if the user has a staff role, otherwise returns false.</returns>
        private bool HasArtccStaffRole(VatusaUserData userData)
        {
            if (userData == null) return false;

            if (userData.data?.roles?.Length >= 1)
            {
                foreach (StaffRole role in userData.data.roles)
                {
                    if (role.facility == "ZLC" && new string[] { "ATM", "DATM", "TA", "EC", "FE", "WM" }.Contains(role.role))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        private bool IsZLCNeighbor(VatusaUserData userModel)
        {
            if (userModel == null) return false;

            if (new string[] { "ZSE", "ZOA", "ZLA", "ZDV", "ZMP" }.Contains(userModel.data?.facility))
            {
                return true;
            }

            return false;
        }

    }
}
