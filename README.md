# CaretakerSteam
SteamID module of the discord bot Caretaker by dabo1#5039 
https://discord.gg/zas9CkW

This is most of the code from caretaker for the steamid module. It has been mostly rewritten, so there are some changes and some missing commands.

If you know how to code:
If you know how to code, I strongly advise you to rewrite the class DataBase.cs . There is an example datastrucure, but since it uses the winodows file system it isn't the fastest and might cause other issues. Also there is in Constants.cs a way to implement a debugging dashboard.

Setup:
The first time you start the bot, it will ask for 2 things:
-Your api key for discord -> needed so the bot can start
-Your discord user id(this is a number not e.g. example#3049, it is something like 265119060331282241) -> this gives you access to certain commands to setup and manage the bot

Once you have done this, you gonna need to add a server list:
Please inform yourself over the laws in your country, you might need to take care of certain data protection laws when processing data gained(especially gdpr)
Option1:
!addserverlist [url]
This url must be in the same formot as http://arkdedicated.com/officialservers.ini .
Option2:
!createserver IP:Port
This adds only 1 server
Once the commands finished running you need to restart the bot one time, this causes it to load the servers

Once this is done, your bot should be ready to go

Tip:
If you want to play ark at the same time and have the bot running, you can change the app id to any other app id in your libary
