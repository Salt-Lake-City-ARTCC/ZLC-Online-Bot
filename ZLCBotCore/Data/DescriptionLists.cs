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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;


namespace ZLCBotCore.Data
{
    public class DescriptionLists
    {
        private int CurrentIndex = -1;
        //private DateTime LastUpdated = DateTime.UtcNow;

        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        private readonly List<string> atcZeroNotes = new List<string>
        {
            "**DID YOU KNOW?...**\n\nIn vERAM, if you hold down CTRL and press the R key, you can cycle through the last commands you made, edit them and submit them again.\n\n*Example:*\n*If I just drew the fixes for the LEEHY STAR (.SLCLEEHYF), then I wanted to open the chart for it, I would simply CTRL+R, underline the last character (F), type C, and press enter.*",
            "**DID YOU KNOW?...**\n\nIn vSTARS, you can press the END key on your keyboard, then Slew two different targets. This will show the minimum distance they are projected to be from each other and it will display the location it will occur.",
            "**DID YOU KNOW?...**\n\nIn vERAM, the Page Up and Down keys on your keyboard serve as a quick command to increase and decrease the vector lines.\n\n*Additionally:*\n*The vector lines will provide you with an approximate position of the aircraft in 1, 2, 4, or 8min from now.\nCareful, as this is based on the CURRENT ground speed and last reported position, and therefore can change during climb or descent.*",
            "**DID YOU KNOW?...**\n\nIn vSTARS, drawing a line on an aircraft using the *P (mileage)(slew) command may be used to help judge distances for separation such as RADAR or wake.\n\n*Tip:*\n*This line can be used as a visual reminder for an aircraft doing something abnormal and requires your special attention. Performing **P removes all of the lines.*",
            "**DID YOU KNOW?...**\n\nIn vERAM, hold down the CTRL button and press the DELETE key; This clears the message response area.",
            "**DID YOU KNOW?...**\n\nIn vSTARS, you can Center-Click on an aircraft to change the color of the FDB.\n\n*Tip:*\n*This can be used as a visual indicator for when an aircraft has read back a communication change instruction or landing clearance.*",
            "**DID YOU KNOW?...**\n\nIn vERAM, Hold down the F9 and F10 keys to display the Aircraft Type/Destination in the 4th Line Data with a FDB.\n\n*Tip:*\n*Make the Aircraft Type part of your normal scan in order to spot inaccurate equipment suffix codes.*",
            "**DID YOU KNOW?...**\n\nIn vSTARS, Performing the *J(mileage)(slew) command will place a 'J-Ball' around the slewed target to help with required RADAR separation",
            "**DID YOU KNOW?...**\n\nIn vERAM, when you complete the QS command for headings and speeds without using the ``(`)`` key, this is information only YOU can see.\nTo have it seen by other ERAM facilities, utilize the Clear Weather Symbol ``(`)``.\n\n*Real World Knowledge:*\n*In the real world, the headings and speeds can be seen by other ERAM facilities without the need for the Clear Weather Symbol and Ross (vERAM developer) is aware of the issue; Hopefully this will be corrected in later software releases.*",
            "**DID YOU KNOW?...**\n\nYou can calculate the Miles Per Min of an aircraft by dropping the last digit of their speed and dividing the remaining digits by 6.\n\n*Example:*\n*ACFT1 has a ground speed of 540kts and in trail is ACFT2 at 480kts.\n\n54/6 = 9, therefore, ACFT1 is traveling at 9 miles per min.\n48/6 = 8, therefore, ACFT2 is traveling at 8 miles per min.\n\nIn 10min, ACFT1 will be 10 miles further along than ACFT2.*",
            "**DID YOU KNOW?...**\n\nIn vERAM, if you accidentally selected a menu item or tear-off and you are moving your mouse, do NOT click anywhere, just hit your ESC key and the item will return to where it was.\n\n",
            "**DID YOU KNOW?...**\n\nIn vERAM, to get data on a non-tracked aircraft, press the HOME button on your keyboard, then LEFT-Click on the target; This will display '.CONTACTME CALLSIGN' in the MCA.\n\nOverstrike starting at the period with SR, space, then hit the DELETE key until the first character of the callsign is underlined.\nNow press enter.\n\nYou now have that aircraft data in your flight plan editor window.\n\n",
            "**DID YOU KNOW?...**\n\nIn vERAM, if an aircraft is calling for services but has no data filed in the VATSIM system, you can do the following command:\nQB (voice code, either V, R, or T) (callsign).\n\nThis will assign a voice code to that aircraft's remarks and will now allow you to perform a SR or VP command to edit their flight data.\n\n*Additionally:*\n*You can use the QB command to assign an equipment suffix code of an aircraft with QB (Code) (FLID) as long as it is not a V, R, or T.*",
            "**DID YOU KNOW?...**\n\nIn vERAM, the command QP J (FLID) will draw a 5 mile 'J-RING' or 'HALO' around the tracked target.\n\nThis can be used to assist with separation.\n\n*Tip:*\n*This can also be used as a visual reminder for an aircraft that needs special attention.*",
            "**DID YOU KNOW?...**\n\nIn vERAM, use QZ to indicate a requested altitude by a pilot and QQ to display what the pilot is currently cleared to climb/descend to.\n\n*Real World Knowledge:*\n*When issuing a crossing restriction such as CROSS SPANE AT FL190, the controller will use QZ for this.\nHowever, if CTR is responsible for issuing Descend Via clearance, the controller will use QQ to display the lowest altitude the pilot is allowed to descend to in their airspace with the DV clearance.*",
            "**DID YOU KNOW?...**\n\nIn vERAM, to more closely align with how real world controllers perform releases and tracks, after issuing a release for departure, complete the DM command for that aircraft and then start a track over the departure point.\n\nThe track will auto-acquire to the target if the aircraft is squawking the correct code and mode.",
            "**DID YOU KNOW?...**\n\nIn vSTARS, by defualt you may only assign an aircraft a scratchpad with up to 3 characters. If you attempt to assign a 4 character scratchpad, it will change the aircraft type to what you type.\n\nTo force vSTARS to accept a 4 character scratchpad, TYPE: `<F7 key> Y <4 character spad> <slew>`",
            "**DID YOU KNOW?...**\n\nWhen you edit a flight plan (even assigning a squawk code), that pilot sometimes can no longer go into the VATSIM system to edit it or file a new flight plan.\nIf this happens to you and you need the pilot to have access:\n\nIn the Text Communications Box, deselect any aircraft you may have selected.\nComplete the following command:\n`.msg fp <callsign> release <enter>`\n\nThis will release the restriction and allow the pilot to refile or edit the flight plan on the VATSIM side.",
            "**DID YOU KNOW?...**\n\nAnother way of calculating separation between aircraft using speed:\n\nTake the first two digits of the ground speed and that is the number of miles that aircraft will travel in 6min.\n\n*Example:*\n*ACFT1 has a ground speed of 580kts and in trail is ACFT2 at 480kts.\n\nTherefore, ACFT1 will travel 58 miles in 6min while ACFT2 only travels 48.\n\nIf ACFT1 was 5miles BEHIND ACFT2 at the time of observcation, in 6min ACFT1 would be LEADING ACFT2 by 5miles.",
        };

        public DescriptionLists(IServiceProvider services)
        {
            _services = services;
            _logger = _services.GetRequiredService<ILogger<DescriptionLists>>();

            //atcZeroNotes = JsonConvert.DeserializeObject<atcZeroNotesJson>(ReadFromGithub("https://raw.githubusercontent.com/Nikolai558/ZLC-DiscordBot/main/ZLCBotCore/atcZeroNotes.json"));
        }

        public string ChooseDescription(bool alwaysChooseDescription = true)
        {
            //if (DateTime.UtcNow.Subtract(LastUpdated).TotalMinutes >= double.Parse(_config["getDescriptionCheck"]))
            //{
            //    //atcZeroNotes = JsonConvert.DeserializeObject<atcZeroNotesJson>(ReadFromGithub("https://raw.githubusercontent.com/Nikolai558/ZLC-DiscordBot/main/ZLCBotCore/atcZeroNotes.json"));
            //    LastUpdated = DateTime.UtcNow;
            //    _logger.LogDebug("Description: Grabed description messages from github.");
            //}

            if (CurrentIndex + 1 > atcZeroNotes.Count() -1)
            {
                CurrentIndex = -1;
            }
            
            CurrentIndex += 1;
            string output = atcZeroNotes[CurrentIndex];
            return output;
        }

        //private string ReadFromGithub(string url)
        //{
        //    string json = "";


        //    using (WebClient webClient = new WebClient()) // TODO - Should this really be inside a using statement?
        //    {
        //        webClient.Headers.Add("Cache-Control", "no-cache");
        //        webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

        //        json = webClient.DownloadString(url);

        //        return json;
        //    }
        //}
    }
}
