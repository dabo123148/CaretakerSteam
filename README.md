# CaretakerSteam
Support: 
https://discord.gg/zas9CkW

What is this?

This is a discord bot, that allows you to monitor player joins and leaves on servers using steamworks. It is mostly meant for ARK: Survival Evolved, but might work for some other steam games aswell that use a similar server setup. 

If you know how to code:

If you know how to code, I strongly advise you to rewrite the class DataBase.cs . There is an example datastrucure, but since it uses the winodows file system it isn't the fastest and might cause other issues. Also there is in Constants.cs a way to implement a debugging dashboard.
You will also need to include a version of steam_api64.dll + steam_appid.txt to your build folder(can be found in release) in order for it to run properly(this is required for the scan function and getting steam names based on their steamid.

Setup:

Make sure the steam desktop client is running and logged into steam friends

The first time you start the bot, it will ask for 2 things:
1. Your api key for discord -> needed so the bot can start
2. Your discord user id(this is a number not e.g. example#3049, it is something like 265119060331282241) -> this gives you access to certain commands to setup and manage the bot

Once you have done this, you gonna need to add a server list:   
Please inform yourself over the laws in your country. You might need to take care of certain data protection laws when processing and collecting data(especially gdpr). There might also be other rules or regulations in place concerning the useage of the steamworks api or other laws that regulate the bot where you live.
1. Option:  
!addserverlist [url]  
This url must be in the same format as http://arkdedicated.com/officialservers.ini .

2. Option:  
!createserver IP:Port   
This adds only 1 server

Once the commands finished running you need to restart the bot one time, this causes it to load the servers

Once this is done, your bot should be ready to go

How does the bot get its data?  
The bot uses 2 ways: 
1. A2S(this is ues for e.g. !server)
2. Steamworks: The bot communicates with the steamclient on your computer, which then communicates with the steamservers and then the bot reads the data from your steamclient -> uses your steamaccount

Tip:
If you want to play ark at the same time and have the bot running, you can change the app id to any other app id in your libary
