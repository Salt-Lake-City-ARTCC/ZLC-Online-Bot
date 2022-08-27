using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZLCBotCore.Data;
using ZLCBotCore.Models.VatsimJsonData;
using ZLCBotCore.Models.VatusaJsonData;

namespace ZLCBotCore.Services
{
    public class VatsimApiService
    {
        private Thread _currentServiceThread;
        private DateTime lastNewPostTime;
        private IUserMessage Message = null;

        public static List<string> ZlcPrefixes { get; protected set; } = new List<string> { "BIL", "BOI", "BZN", "SUN", "GPI", "GTF", "HLN", "IDA", "JAC", "TWF", "MSO", "OGD", "PIH", "PVU", "SLC", "ZLC" };
        public static List<string> Suffixes { get; protected set; } = new List<string> { "DEL", "GND", "TWR", "APP", "DEP", "CTR", "TMU" };

        public bool VatsimServiceRun { get; protected set; } = false;
        
        private readonly IConfiguration _config;
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _discord;
        private readonly ILogger _logger;
        private readonly ControllerLists _controllerLists;

        //public List<VatsimController> ZLCOnlineControllers { get; protected set; }

        public VatsimApiService(IServiceProvider services)
        {
            _services = services;
            _config = _services.GetRequiredService<IConfiguration>();
            _logger = _services.GetRequiredService<ILogger<CommandHandler>>();
            _controllerLists = services.GetRequiredService<ControllerLists>();
            _discord = services.GetRequiredService<DiscordSocketClient>();

            //_channel = _discord.GetChannel(964591927028752446); // Kyle Server 964591927028752446
            //_channel = _discord.GetChannel(925988386358042644); // ZLC Server 

            _logger.LogInformation("Loaded: VatsimApiService");
        }

        private async void Run()
        {
            _logger.LogInformation("Service: VatsimApiService.Run Started");

            if (_config.GetSection("includeOBS").Value == "true")
            {
                _logger.LogDebug("Service: VatsimApiService is Including Observer positions.");
                Suffixes.Add("OBS");
            }

            while (VatsimServiceRun)
            {
                var online = GetOnlineControllers();
                Thread.Sleep(1000);

                if (online != null)
                {
                    _controllerLists.ZLCOnlineControllers = online;
                }

                await UpdateDiscordMessage(_discord.GetChannel(Convert.ToUInt64(_config.GetRequiredSection("discordChannel").Value)) as IMessageChannel); // ZLC Channel.

                Thread.Sleep(int.Parse(_config.GetSection("serviceCheckLimit").Value));
            }
        }

        public Task Start()
        {
            _logger.LogDebug("Function: VatsimApiService.Start() Called");

            // Just incharge of reaching out to Vatisim and Vatusa API and keeping a list of who is online! 
            VatsimServiceRun = true;

            _currentServiceThread = new Thread(Run);
            _currentServiceThread.Start();

            return Task.CompletedTask;
        }

        public void Stop()
        {
            _logger.LogWarning("Function: VatsimApiService.Stop() Called");

            _currentServiceThread.Abort();

            VatsimServiceRun = false;
        }

        private List<VatsimController> GetOnlineControllers()
        {
            _logger.LogDebug("Function: VatsimApiService.GetOnlineControllers() Called");

            // Vatsim Json Link: https://data.vatsim.net/v3/vatsim-data.json

            string vatsimJsonString = ReadJsonFromWebsite("https://data.vatsim.net/v3/vatsim-data.json");

            VatsimJsonRootModel AllVatsimInfo = JsonConvert.DeserializeObject<VatsimJsonRootModel>(vatsimJsonString); // TODO - .Net Core has Json Functions in it. Switch to using that instead of Netonsoft.
            if (AllVatsimInfo is null)
            {
                _logger.LogError("Json: Could not Deserialize Vatsim Json. Is the website down?");
                return null;
            }

            List<VatsimController> OnlineControllers = new List<VatsimController>();

            foreach (VatsimController controller in AllVatsimInfo.controllers)
            {
                if (controller.callsign.Contains('_'))
                {
                    string[] CallsignSplit = controller.callsign.Split('_');
                    string currentControllerPrefix = CallsignSplit[0];
                    string currentControllerSuffix = CallsignSplit[^1];

                    if ((_config.GetSection("debuging").GetSection("allowAnyPrefix").Value == "true" || ZlcPrefixes.Contains(currentControllerPrefix)) 
                        && (_config.GetSection("debuging").GetSection("allowAnySuffix").Value == "true" || Suffixes.Contains(currentControllerSuffix)))
                    {
                        string old_name = controller.name;

                        // check current list and fix online list UpdatedNameWithVatUsa bool.
                        if (_controllerLists.CurrentPostedControllers.Count >= 1)
                        {
                            foreach (VatsimController postedController in _controllerLists.CurrentPostedControllers)
                            {
                                if (postedController.cid == controller.cid)
                                {
                                    controller.UpdatedNameWithVatUsa = postedController.UpdatedNameWithVatUsa;
                                    controller.name = postedController.name;
                                    break;
                                }
                            }
                        }

                        try
                        {
                            if (_config.GetSection("corectName").Value == "true" && !controller.UpdatedNameWithVatUsa)
                            {
                                string new_name = GetControllerName(controller.cid);

                                if (new_name != null)
                                {
                                    controller.name = new_name;
                                    controller.UpdatedNameWithVatUsa = true;
                                    _logger.LogDebug($"Name: Controller name Changed [{old_name}] -> [{new_name}]");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"Name: Could not change Controller Name [{controller.name}]: {e.Message}");
                            if (string.IsNullOrWhiteSpace(old_name))
                            {
                                controller.name = "UNKNOWN NAME";
                            }
                            else
                            {
                                controller.name = old_name;
                            }
                        }

                        OnlineControllers.Add(controller);
                    }
                }
            }

            return OnlineControllers;
        }

        private string GetControllerName(int cid)
        {
            _logger.LogDebug($"Function: VatsimApiService.GetControllerName() Called **args[{cid}]");

            // Vatusa API link: https://api.vatusa.net/v2/user/{cid}

            string VatusaJsonString = ReadJsonFromWebsite($"https://api.vatusa.net/v2/user/{cid}");

            VatusaJsonRoot ControllerInformation = JsonConvert.DeserializeObject<VatusaJsonRoot>(VatusaJsonString); // TODO - .Net Core has Json Functions in it. Switch to using that instead of Netonsoft.
            if (ControllerInformation is null)
            {
                _logger.LogError("Json: Could not Deserialize Vatusa Json. Is the website down?");
                return null;
            }

            string ControllerFullName = $"{ControllerInformation.data.fname} {ControllerInformation.data.lname}";

            return ControllerFullName;
        }

        private string ReadJsonFromWebsite(string url)
        {
            _logger.LogDebug($"Function: VatsimApiService.ReadJsonFromWebsite() Called **args[{url}]");

            using (WebClient webClient = new WebClient()) // TODO - Should this really be inside a using statement?
            {
                try
                {
                    string json = webClient.DownloadString(url);

                    return json;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Function: VatsimApiService.ReadJsonFromWebsite() Called **args[{url}] - {ex.Message}");
                    return null;
                }
                
            }
        }

        private async Task UpdateDiscordMessage(IMessageChannel guildChannel)
        {
            _logger.LogInformation("Service: UpdateDiscordMessage Started");

            var _message = await FindBotMessage(guildChannel);

            if (_message == null)
            {
                _controllerLists.CurrentPostedControllers = _controllerLists.ZLCOnlineControllers;

                EmbedBuilder MessageText = FormatDiscordMessage();
                
                if(NewControllerLoggedOn()) MessageText.Description = "My appologies for the notification, but I was unable to find my previous message. One of your pesky staff members must have deleted it.\n\n" + MessageText.Description;

                Message = await guildChannel.SendMessageAsync("", false, MessageText.Build());
                lastNewPostTime = DateTime.UtcNow;
                return;
            }
            else
            {
                Message = _message;
            }

            if (NewControllerLoggedOn())
            {
                if (DateTime.UtcNow.Subtract(lastNewPostTime).TotalMinutes >= double.Parse(_config.GetSection("newPostLimit").Value))
                {
                    _controllerLists.CurrentPostedControllers = _controllerLists.ZLCOnlineControllers;
                    EmbedBuilder MessageText = FormatDiscordMessage();

                    if (Message != null)
                    {
                        // try to delete the previous message
                        try { await Message.DeleteAsync(); } catch (Exception ex) { _logger.LogError($"Message: {ex.Message}"); }
                    }

                    Message = await guildChannel.SendMessageAsync("", false, MessageText.Build());
                    lastNewPostTime = DateTime.UtcNow;
                }
                else
                {
                    _controllerLists.CurrentPostedControllers = _controllerLists.ZLCOnlineControllers;
                    EmbedBuilder MessageText = FormatDiscordMessage();

                    await Message.ModifyAsync(msg => msg.Embed = MessageText.Build());
                }
            }
            else
            {
                _controllerLists.CurrentPostedControllers = _controllerLists.ZLCOnlineControllers;
                EmbedBuilder MessageText = FormatDiscordMessage();

                await Message.ModifyAsync(msg => msg.Embed = MessageText.Build());
            }
        }

        private async Task<IUserMessage> FindBotMessage(IMessageChannel channel)
        {
            var messages = await channel.GetMessagesAsync().FlattenAsync();
            IUserMessage result = null;

            int botMessageCount = 0;
            foreach (var message in messages)
            {
                var _message = message as IUserMessage;

                if (_message == null) return null;

                if (_message.Author.IsBot && _message.Author.Id == 923302257603252324)
                {
                    botMessageCount++;
                    if (botMessageCount == 1) result = _message;
                    if (botMessageCount > 1)
                    {
                        await _message.DeleteAsync();
                    }
                }
            }

            return result;
        }

        private bool NewControllerLoggedOn()
        {
            _logger.LogDebug("Function: OnlineControllerLogic.NewControllerLoggedOn() Called");

            // ExtractCidFromLists Looks at the online controller list (current) and the Posted Controller List (previous)
            Dictionary<string, List<int>> CidLists = ExtractCidFromLists();

            IEnumerable<int> differenceQuery = CidLists["OnlineCids"].Except(CidLists["PostedCids"]);

            if (differenceQuery.Count() == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private Dictionary<string, List<int>> ExtractCidFromLists()
        {
            _logger.LogDebug("Function: OnlineControllerLogic.ExtractCidFromLists() Called");

            var controllerCids = new Dictionary<string, List<int>>(){
                {"PostedCids", new List<int>()},
                {"OnlineCids", new List<int>()}
            };

            foreach (VatsimController currentController in _controllerLists.CurrentPostedControllers)
            {
                controllerCids["PostedCids"].Add(currentController.cid);
            }

            foreach (VatsimController onlineController in _controllerLists.ZLCOnlineControllers)
            {
                controllerCids["OnlineCids"].Add(onlineController.cid);
            }

            return controllerCids;
        }

        private EmbedBuilder FormatDiscordMessage()
        {
            _logger.LogDebug("Function: OnlineControllerLogic.FormatDiscordMessage() Called");
            var time = DateTime.UtcNow.ToString("HH:mm");

            var embed = new EmbedBuilder();

            embed.Title = $"{_controllerLists.CurrentPostedControllers.Count}  -  ATC ONLINE";
            embed.Color = new Discord.Color(0, 38, 0);
            embed.Footer = new EmbedFooterBuilder { Text = $"Updated: {time}z" };

            if (_controllerLists.CurrentPostedControllers.Count() <= 0)
            {
                embed.Title = "NO ATC ONLINE";
                embed.Color = new Discord.Color(38, 0, 0);
                embed.Description = _services.GetRequiredService<DescriptionLists>().ChooseDescription();
                return embed;
            }

            Dictionary<string, List<VatsimController>> atcBySuffix = new Dictionary<string, List<VatsimController>>
            {
                { "TMU", new List<VatsimController>() },
                { "CTR", new List<VatsimController>() },
                { "APP", new List<VatsimController>() },
                { "DEP", new List<VatsimController>() },
                { "TWR", new List<VatsimController>() },
                { "GND", new List<VatsimController>() },
                { "DEL", new List<VatsimController>() },
                { "OBS", new List<VatsimController>() },
                { "OTHER", new List<VatsimController>() }
            };

            foreach (var onlineController in _controllerLists.CurrentPostedControllers)
            {
                var splitCallsign = onlineController.callsign.Split('_');
                var suffix = splitCallsign[^1];

                if (atcBySuffix.Keys.Contains(suffix))
                {
                    atcBySuffix[suffix].Add(onlineController);
                }
                else
                {
                    atcBySuffix["OTHER"].Add(onlineController);

                }
            }

            foreach (var suffix in atcBySuffix.Keys)
            {
                bool test = false;
                string valueForField = $"";
                foreach (VatsimController controller in atcBySuffix[suffix])
                {
                    string addToField = $"***{controller.callsign}*** - {controller.name}\n";

                    if (valueForField.Length + addToField.Length + $"{'\u200B'}\n{'\u200B'}\n".Length > 1024)
                    {
                        //valueForField += $"{'\u200B'}\n{'\u200B'}\n";
                        AddField(embed, valueForField, suffix, test);
                        test = true;
                        valueForField = "";
                    }

                    valueForField += addToField;
                }

                valueForField += $"{'\u200B'}\n{'\u200B'}\n";

                AddField(embed, valueForField, suffix, test);
            }

            return embed;
        }

        private void AddField(EmbedBuilder embed, string fieldValue, string suffix, bool continuationOfCategory)
        {
            switch (continuationOfCategory)
            {
                case false:
                    {
                        if ((!string.IsNullOrEmpty(fieldValue) && !string.IsNullOrWhiteSpace(fieldValue) && fieldValue != $"{'\u200B'}\n{'\u200B'}\n")
                             && (!string.IsNullOrWhiteSpace(suffix) && !string.IsNullOrEmpty(suffix)))
                        {
                            switch (suffix)
                            {
                                case "TMU": { embed.AddField(new EmbedFieldBuilder { Name = $"**__TFC Management__**", Value = $"{fieldValue}" }); break; }
                                case "CTR": { embed.AddField(new EmbedFieldBuilder { Name = $"**__Center__**", Value = $"{fieldValue}" }); break; }
                                case "APP": { embed.AddField(new EmbedFieldBuilder { Name = $"**__Approach__**", Value = $"{fieldValue}" }); break; }
                                case "DEP": { embed.AddField(new EmbedFieldBuilder { Name = $"**__Departure__**", Value = $"{fieldValue}" }); break; }
                                case "GND": { embed.AddField(new EmbedFieldBuilder { Name = $"**__Ground__**", Value = $"{fieldValue}" }); break; }
                                case "DEL": { embed.AddField(new EmbedFieldBuilder { Name = $"**__Clearance__**", Value = $"{fieldValue}" }); break; }
                                case "TWR": { embed.AddField(new EmbedFieldBuilder { Name = $"**__Tower__**", Value = $"{fieldValue}" }); break; }
                                case "OBS": { embed.AddField(new EmbedFieldBuilder { Name = $"**__Observer__**", Value = $"{fieldValue}" }); break; }
                                default: { embed.AddField(new EmbedFieldBuilder { Name = $"**__Other__**", Value = $"{fieldValue}" }); break; }
                            }
                        }

                        break;
                    }
                case true:
                    {
                        if ((!string.IsNullOrEmpty(fieldValue) && !string.IsNullOrWhiteSpace(fieldValue) && fieldValue != $"{'\u200B'}\n{'\u200B'}\n")
                             && (!string.IsNullOrWhiteSpace(suffix) && !string.IsNullOrEmpty(suffix)))
                        {
                            switch (suffix)
                            {
                                case "TMU": { embed.AddField(new EmbedFieldBuilder { Name = $"**__TFC Management-Cont.__**", Value = $"{fieldValue}" }); break; }
                                case "CTR": { embed.AddField(new EmbedFieldBuilder { Name = $"**__Center-Cont.__**", Value = $"{fieldValue}" }); break; }
                                case "APP": { embed.AddField(new EmbedFieldBuilder { Name = $"**__Approach-Cont.__**", Value = $"{fieldValue}" }); break; }
                                case "DEP": { embed.AddField(new EmbedFieldBuilder { Name = $"**__Departure-Cont.__**", Value = $"{fieldValue}" }); break; }
                                case "GND": { embed.AddField(new EmbedFieldBuilder { Name = $"**__Ground-Cont.__**", Value = $"{fieldValue}" }); break; }
                                case "DEL": { embed.AddField(new EmbedFieldBuilder { Name = $"**__Clearance-Cont.__**", Value = $"{fieldValue}" }); break; }
                                case "TWR": { embed.AddField(new EmbedFieldBuilder { Name = $"**__Tower-Cont.__**", Value = $"{fieldValue}" }); break; }
                                case "OBS": { embed.AddField(new EmbedFieldBuilder { Name = $"**__Observer-Cont.__**", Value = $"{fieldValue}" }); break; }
                                default: { embed.AddField(new EmbedFieldBuilder { Name = $"**__Other-Cont.__**", Value = $"{fieldValue}" }); break; }
                            }
                        }
                        break;
                    }
            }
        }
    }
}
