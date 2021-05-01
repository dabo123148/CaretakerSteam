# CaretakerSteam
SteamID module of the discord bot Caretaker by dabo1#5039 
https://discord.gg/zas9CkW

What is this?

This is a discord bot, that allows you to monitor player joins and leaves on servers using steamworks. It is mostly meant for ARK: Survival Evolved, but might work for some other steam games aswell that use a similar server setup. 

If you know how to code:

If you know how to code, I strongly advise you to rewrite the class DataBase.cs . There is an example datastrucure, but since it uses the winodows file system it isn't the fastest and might cause other issues. Also there is in Constants.cs a way to implement a debugging dashboard.

Setup:

Make sure the steam desktop client is running and logged into steam friends

The first time you start the bot, it will ask for 2 things:
1. Your api key for discord -> needed so the bot can start
2. Your discord user id(this is a number not e.g. example#3049, it is something like 265119060331282241) -> this gives you access to certain commands to setup and manage the bot

Once you have done this, you gonna need to add a server list:

Please inform yourself over the laws in your country, you might need to take care of certain data protection laws when processing and collecting data(especially gdpr)
1. Option:
!addserverlist [url]
This url must be in the same formot as http://arkdedicated.com/officialservers.ini .
2. Option:
!createserver IP:Port
This adds only 1 server
Once the commands finished running you need to restart the bot one time, this causes it to load the servers

Once this is done, your bot should be ready to go

Tip:
If you want to play ark at the same time and have the bot running, you can change the app id to any other app id in your libary
