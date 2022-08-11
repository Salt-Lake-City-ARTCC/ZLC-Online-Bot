# ZLC-DiscordBot

SUMMARY:

The “ZLC-Online Bot” will make automated posts to the ZLC Discord channel “online-atc”, notifying channel subscribers of ATC that have signed on, whilst maintaining an updated roster of current online controllers every 5min.



LOGIC:

- The bot will check to see who is online once every 5min.
- A controller has signed onto a ZLC position that was not online in the previous check.
   - False:
     - The bot will edit the previous post to display all of the current controllers online. (without notifying the channel subscribers)
   - True:
     - It has been at least 15min since the last new Discord post.
       - False:
         - The bot will edit the previous post to display all of the current controllers online. (without notifying the channel subscribers)
       - True:
         - The bot will delete the previous post and submit a new post with all of the current ZLC controllers online. (notifying the channel subscribers)
