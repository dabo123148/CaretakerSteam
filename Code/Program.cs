using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Threading;
using System.Net;
using System.IO;

namespace Caretaker
{

    public class Program
    {
        static void Main(string[] args)
        {
            string token;
            if (File.Exists(AppContext.BaseDirectory + "\\Token.txt"))
            {
                token = File.ReadAllText(AppContext.BaseDirectory + "\\Token.txt");
            }
            else
            {
                Console.Write("Please enter the api key for your discord bot\nToken:");
                token = Console.ReadLine();
                File.WriteAllText(AppContext.BaseDirectory + "\\Token.txt",token);
            }
            ulong puserid;
            if (File.Exists(AppContext.BaseDirectory + "\\HostID.txt"))
            {
                puserid = ulong.Parse(File.ReadAllText(AppContext.BaseDirectory + "\\HostID.txt"));
            }
            else
            {
                Console.Write("Please enter your discord userid(this gives access to certain management commands)\nID:");
                if(!ulong.TryParse(Console.ReadLine(),out puserid) || puserid<10000)
                {
                    Console.WriteLine("This is not a valid userid");
                    return;
                }
                File.WriteAllText(AppContext.BaseDirectory + "\\HostID.txt", puserid.ToString());
            }
            new Program().MainAsync(token,puserid).GetAwaiter().GetResult();
        }
        public DiscordSocketClient _client;
        public SteamIDAnalyzer steamIDControll;
        private string Token = "";
        public Compfort C;
        private ulong AuthorUserID;
        public async Task MainAsync(string pToken,ulong author)
        {
            AuthorUserID = author;
            Token = pToken;
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                GuildSubscriptions = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });
            _client.SetGameAsync("support: " + Constants.supportserver);
            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, pToken);
            await _client.StartAsync();
            _client.MessageReceived += Client_MessageReceived;
            _client.LeftGuild += Client_LeftGuild;
            _client.Disconnected += OnClientDisconnect;
            _client.ChannelDestroyed += Client_OnChannelDeleted;
            C = new Compfort(this);
            C.postMessageTask();
            steamIDControll = new SteamIDAnalyzer(this);
            try
            {
                steamIDControll.ServerScannerNew();
            }
            catch (Exception)
            {
                Compfort.Log("CRITITCAL ERROR IN SERVERSCANNER", LogType.Crash);
            }
            await Task.Delay(-1);
        }
        /// <summary>
        /// Called when the bot loses the connection to the discord servers, this restablishes the connection
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private async Task OnClientDisconnect(Exception ex)
        {
            bool reconnected = false;
            while (reconnected == false)
            {
                try
                {
                    Compfort.Log("ONCLIENTDISCONNECT");
                    Compfort.Log("DISCONNECTED WITH EXCEPTION " + ex.Message);
                    _client.Dispose();
                    _client = new DiscordSocketClient(new DiscordSocketConfig
                    {
                        AlwaysDownloadUsers = true,
                        GuildSubscriptions = true,
                        DefaultRetryMode = RetryMode.AlwaysRetry
                    });
                    _client.SetGameAsync("support: " + Constants.supportserver);
                    _client.Log += Log;
                    await _client.LoginAsync(TokenType.Bot, Token);
                    await _client.StartAsync();
                    _client.MessageReceived += Client_MessageReceived;
                    _client.LeftGuild += Client_LeftGuild;
                    _client.Disconnected += OnClientDisconnect;
                    _client.ChannelDestroyed += Client_OnChannelDeleted;
                    reconnected = true;
                }
                catch (Exception ex2)
                {
                    Thread.Sleep(15000);
                    Compfort.Log("Error when reconnecting to discord server");
                    Compfort.Log(ex2.Message + "\n" + ex2.StackTrace, LogType.Crash); ;
                    reconnected = false;
                }
            }
        }
        /// <summary>
        /// Logs discord events
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private Task Log(LogMessage msg)
        {
            Compfort.Log(msg.ToString(), LogType.Other);
            return Task.CompletedTask;
        }
        private Task Client_LeftGuild(SocketGuild guild)
        {
            Compfort.Log("Bot has been removed from " + guild.Name + " (ID: " + guild.Id + ")");
            DataBase.DeleteGuildData(guild.Id);
            return Task.CompletedTask;
        }
        private Task Client_OnChannelDeleted(SocketChannel channel)
        {
            DataBase.DeleteChannelData(channel.Id);
            return Task.CompletedTask;
        }
        private async Task Client_MessageReceived(SocketMessage message)
        {
            SocketGuildChannel guildchannel = message.Channel as SocketGuildChannel;
            //Prevents infinite selfcallback
            if (message.Author.IsBot) return;
            //Prevents commands in pms
            if (guildchannel == null && !message.Author.IsBot)
            {
                await message.Channel.SendMessageAsync("The bot does not accept commands in pms");
                return;
            }
            string Content = message.Content.ToLower();
            ulong guildid = guildchannel.Guild.Id;
            //reading out choices(e.g. for !scan command)
            if (message.Content != "" && int.TryParse(message.Content, out int choice))
            {
                string choicestring = DataBase.GetChoice(message.Channel.Id,message.Author.Id, choice);
                if (choicestring.Length > 0) await message.Channel.SendMessageAsync(choicestring);
            }
            //Filters out all non commands -> no need to process them anyway
            if (message.Content.Length < 2 || message.Content[0] != '!') return;
            switch (Content.Split(' ')[0])
            {
                case "!help":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    HelpCommand(message, guildid, Content);
                    break;
                case "!server":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    ServerCommand(message, guildid, Content);
                    break;
                case "!deactivate":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    DeactivateCommand(message, guildid, Content);
                    break;
                case "!activate":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    ActivateCommand(message, guildid, Content);
                    break;
                case "!createtribe":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    CreateTribeCommand(message, guildid, Content);
                    break;
                case "!addserver":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    AddTribeToServerCommand(message, guildid, Content);
                    break;
                case "!removeserver":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    RemoveTribeFromServerCommand(message, guildid, Content);
                    break;
                case "!sid":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SidCommand(message, guildid, Content);
                    break;
                case "!renametribe":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    RenameTribeCommand(message, guildid, Content);
                    break;
                case "!setrelationship":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SetRelationshipCommand(message, guildid, Content, false);
                    break;
                case "!setallrelationship":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SetRelationshipCommand(message, guildid, Content, true);
                    break;
                case "!setgamemode":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SetGamemodeCommand(message, guildid, Content);
                    break;
                case "!toggleleavelog":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    ToggleLeavelogCommand(message, guildid, Content);
                    break;
                case "!setalertoutput":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SetAlertOutputCommand(message, guildid, Content);
                    break;
                case "!setsid":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SetSteamIDCommand(message, guildid, Content);
                    break;
                case "!setsteamid":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SetSteamIDCommand(message, guildid, Content);
                    break;
                case "!sidremovetribe":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SteamIDDeleteTribeCommand(message, guildid, Content);
                    break;
                case "!sidrt":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SteamIDDeleteTribeCommand(message, guildid, Content);
                    break;
                case "!sidremoverelationship":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SteamIDDeleteRelationshipCommand(message, guildid, Content);
                    break;
                case "!sidrr":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SteamIDDeleteRelationshipCommand(message, guildid, Content);
                    break;
                case "!sidremoverename":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SteamIDDeleteNameCommand(message, guildid, Content);
                    break;
                case "!sidrn":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SteamIDDeleteNameCommand(message, guildid, Content);
                    break;
                case "!deletesteamid":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SteamIDDeleteCommand(message, guildid, Content);
                    break;
                case "!listtribes":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    ListTribesCommand(message, guildid, Content);
                    break;
                case "!listalerts":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    ListAlertsCommand(message, guildid, Content);
                    break;
                case "!scan":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    ScanCommand(message, guildid, Content);
                    break;
                case "!deletetribe":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    DeleteTribeCommand(message, guildid, Content, false);
                    break;
                case "!deletetribeforce":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    DeleteTribeCommand(message, guildid, Content, true);
                    break;
                case "!tribeinfo":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    TribeinfoCommand(message, guildid, Content);
                    break;
                case "!steamidimport":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SteamIDImportCommand(message, guildid, Content);
                    break;
                case "!steamidexport":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    SteamIDExportCommand(message, guildid, Content);
                    break;
                case "!devhelp":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    DevHelpCommand(message, guildid, Content);
                    break;
                case "!markbot":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    MarkBotCommand(message, guildid, Content);
                    break;
                case "!addserverlist":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    LoadServerListCommand(message, guildid, Content);
                    break;
                case "!createserver":
                    Compfort.Log("Command in " + guildchannel.Guild.Name + " (" + guildchannel.Guild.Id + ") by " + message.Author + "(" + message.Author.Id + "):" + message.Content, LogType.Command);
                    CreateServerCommand(message, guildid, Content);
                    break;
            }
        }
        private void SidCommand(SocketMessage message, ulong guildid, string Content)
        {
            string[] elements = Content.Split(' ');
            if (elements.Length < 2)
            {
                message.Channel.SendMessageAsync("Command invalid. Use !sid [SteamID] [from days ago(optional)] [till days ago(optional)]");
                return;
            }
            if (!Compfort.IsSteamID(elements[1]))
            {
                message.Channel.SendMessageAsync("SteamID is invalid, Steamids have 17 charaters and start with 7656... . Try https://steamid.io/ if you are having issue finding it");
                return;
            }
            if (elements[1].CompareTo("76561197960287930") == 0)
            {
                message.Channel.SendMessageAsync("SteamID is invalid, this is the example steamid from https://steamid.io/");
                return;
            }
            int enddays = 14;
            int startdays = 0;
            if (elements.Length == 3 && int.TryParse(elements[2], out int penddays))
            {
                enddays = penddays;
            }
            if (elements.Length == 4 && int.TryParse(elements[2], out int pstartdays) && int.TryParse(elements[3], out penddays))
            {
                startdays = pstartdays;
                enddays = penddays;
            }
            if (startdays < 0) startdays = 0;
            if (enddays <= startdays) enddays = startdays + 1;
            message.Channel.SendMessageAsync(steamIDControll.PresentSteamIDData(guildid, ulong.Parse(Content.Split(' ')[1]), enddays, startdays));
        }
        private void RemoveTribeFromServerCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Split(' ').Length < 3)
            {
                message.Channel.SendMessageAsync("Invalid parameters. Use ```css\n!removeserver [server] [Tribename]```");
                return;
            }
            List<ServerData> sData = steamIDControll.FindServer(Content.Split(' ')[1], DataBase.GetGameMode(guildid));
            if (sData.Count() == 0)
            {
                message.Channel.SendMessageAsync(Constants.servernotfoundmessage);
                return;
            }
            string name = message.Content.Split(' ')[2];
            for (int A = 3; A < message.Content.Split(' ').Length; A++)
            {
                name += " " + message.Content.Split(' ')[A];
            }
            name = Compfort.RemoveChars(name, Constants.InvalidTribenameCharacters);
            if (sData.Count() != 1)
            {
                message.Channel.SendMessageAsync(Compfort.MultipleServersFound(sData, message.Author.Id, message.Channel.Id, "!removeserver ", " " + name));
                return;
            }
            List<TribeData> knowntribes = DataBase.GetTribes(guildid, sData.First());
            if (knowntribes.Count > 0)
            {
                foreach (TribeData tribe in knowntribes)
                {
                    if (tribe.name.ToLower().CompareTo(name.ToLower()) == 0)
                    {
                        name = tribe.name;
                        DataBase.RemoveTribeFromServer(guildid, sData.First(), tribe);
                        message.Channel.SendMessageAsync(name + " has been removed from server " + sData.First().Name);
                        return;
                    }
                }
                message.Channel.SendMessageAsync("The tribe doesn't exist or hasn't been added to this server. Check !listtribes and !server, to make sure it is added");
                return;
            }
            else
            {
                if (DataBase.GetTribes(guildid).Count == 0)
                {
                    message.Channel.SendMessageAsync("No tribes have been created yet or . Use \"!createtribe\" first");
                }
                else
                {
                    message.Channel.SendMessageAsync("The server doesn't have any tribes, so there is no tribe to be removed");
                }
                return;
            }
        }
        private void AddTribeToServerCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Split(' ').Length < 3)
            {
                message.Channel.SendMessageAsync("Invalid parameters. Use ```css\n!addserver [server] [Tribename]```");
                return;
            }
            List<ServerData> sData = steamIDControll.FindServer(Content.Split(' ')[1], DataBase.GetGameMode(guildid));
            if (sData.Count() == 0)
            {
                message.Channel.SendMessageAsync(Constants.servernotfoundmessage);
                return;
            }
            string name = message.Content.Split(' ')[2];
            for (int A = 3; A < message.Content.Split(' ').Length; A++)
            {
                name += " " + message.Content.Split(' ')[A];
            }
            name = Compfort.RemoveChars(name, Constants.InvalidTribenameCharacters);
            if (sData.Count() != 1)
            {
                message.Channel.SendMessageAsync(Compfort.MultipleServersFound(sData, message.Author.Id, message.Channel.Id, "!addserver ", " " + name));
                return;
            }
            List<TribeData> knowntribes = DataBase.GetTribes(guildid);
            if (knowntribes.Count > 0)
            {
                foreach (TribeData tribe in knowntribes)
                {
                    if (tribe.name.ToLower().CompareTo(name.ToLower()) == 0)
                    {
                        name = tribe.name;
                        DataBase.AddTribeToServer(guildid, sData.First(), tribe);
                        string tribesaddedbefore = "";
                        if (DataBase.GetTribes(guildid, sData.First()).Count > 1) tribesaddedbefore = ". Incase you are unaware, there are already tribes added to that server before.";
                        message.Channel.SendMessageAsync(name + " has been added to server " + sData.First().Name + tribesaddedbefore);
                        return;
                    }
                }
                message.Channel.SendMessageAsync(Constants.unknowntribemessage);
                return;
            }
            else
            {
                message.Channel.SendMessageAsync("No tribes have been created yet. Use \"!createtribe\" first");
                return;
            }
        }
        private void HelpCommand(SocketMessage message,ulong guildid,string Content)
        {
            string helpmessage = "";
            helpmessage += "Looks up a SteamID```css\n!sid [SteamID] [days(optional)] [days(optional)]```";
            helpmessage += "Looks up Server```css\n!server [Servername]```";
            helpmessage += "Lists all SteamIDs on a server```css\n!scan [Servername] [filtered(default=false)]```";
            helpmessage += "Creates a tribe```css\n!createtribe [Relationship] [Tribename]```";
            helpmessage += "Deletes a tribe```css\n!deletetribe [Tribename]```";
            helpmessage += "Renames a tribe```css\n!renametribe [old Tribename] [new Tribename]```";
            helpmessage += "Lists all tribes```css\n!listtribes```";
            helpmessage += "Adds a tribe to a server```css\n!addserver [Servername] [Tribename]```";
            helpmessage += "Removes a tribe from a server```css\n!removeserver [Servername] [Tribename]```";
            helpmessage += "Lists all data of a tribe```css\n!tribeinfo [Tribename]```";
            helpmessage += "Sets a player```css\n!setsteamid [SteamID] [Relationship(optional)] [Tribename(optional)] [Name(optional)]```";
            helpmessage += "Removes the tribe of a steamid```css\n!sidremovetribe [SteamID]```";
            helpmessage += "Removes the name of a steamid```css\n!sidremovename [SteamID]```";
            helpmessage += "Removes the relationship of a steamid```css\n!sidremoverelationship [SteamID]```";
            helpmessage += "Deletes a player out of the database```css\n!deletesteamid [SteamID]```";
            helpmessage += "Adds alerts for a certain server```css\n!activate steamalert [Servername] [Relationship] [Mentioning(optional)]```";
            helpmessage += "Deletes alerts for a certain server```css\n!deactivate steamalert [Servername] [Relationship]```";
            helpmessage += "Changes a tribe relationship```css\n!setrelationship [Relationship] [Tribename]```";
            helpmessage += "Changes a tribe relationship including all steamids that were given a relationship and this tribe```css\n!setallrelationship [Relationship] [Tribename]```";
            helpmessage += "Lists all alerts ```css\n!listalerts```";
            if (helpmessage.Length > 2000)
            {
                Compfort.Log("Help too long (" + helpmessage.Length + ")");
                helpmessage = "Helpmessage too long, please contact the person maintaining the bot";
            }
            message.Channel.SendMessageAsync(helpmessage);
            helpmessage = "";
            helpmessage += "Sets the gamemode(only servers of that set gamemode can be found) ```css\n!setgamemode [all, core, classic, conquest, small, crossark]```";
            helpmessage += "Sets the format in which alert messages are beeing presented ```css\n!setalertoutput [short,detailed]```";
            helpmessage += "Exports all manually added ids, that match the filter. Can be importet by other bots```css\n!steamidexport [Tribename or all]```";
            helpmessage += "Imports all manually added ids```css\n!steamidimport (export file as attachment)```";
            helpmessage += "Activates/Deactivates leavemessages```css\n!toggleleavelog```";
            if (message.Author.Id == AuthorUserID) helpmessage += "List of commands only you can execute(dev commands)```css\n!devhelp```";
            helpmessage += "Open Source Github implementation from: " + Constants.githuburl + "\n";
            helpmessage += "Support server: " + Constants.supportserver + "\n";
            if (helpmessage.Length > 2000)
            {
                Compfort.Log("Help too long (" + helpmessage.Length + ")");
                helpmessage = "Helpmessage too long, please contact the person maintaining the bot";
            }
            message.Channel.SendMessageAsync(helpmessage);
        }
        private void DevHelpCommand(SocketMessage message, ulong guildid, string Content)
        {
            string helpmessage = "";
            helpmessage += "Adds a server```css\n!createserver [IP:Port]```";
            helpmessage += "Adds a server list(must be format as http://arkdedicated.com/officialservers.ini) ```css\n!addserverlist [url]```";
            helpmessage += "Adds a bot/makes an id invisible for the bot```!addbot [SteamID]```";
            if (helpmessage.Length > 2000)
            {
                Compfort.Log("Help too long (" + helpmessage.Length + ")");
                helpmessage = "Helpmessage too long, please contact the person maintaining the bot";
            }
            message.Channel.SendMessageAsync(helpmessage);
        }
        private void CreateTribeCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Split(' ').Length < 3)
            {
                message.Channel.SendMessageAsync("Invalid parameters. Use ```css\n!createtribe [relationship] [Tribename]```");
                return;
            }
            int relationshipint = 0;
            if (Content.Split(' ')[1] == "enemy") relationshipint = 1;
            if (Content.Split(' ')[1] == "neutral") relationshipint = 2;
            if (Content.Split(' ')[1] == "allied" || Content.Split(' ')[1] == "ally") relationshipint = 3;
            if (Content.Split(' ')[1] == "friendly") relationshipint = 4;
            if (Content.Split(' ')[1] == "tribe" || Content.Split(' ')[1] == "tribemember") relationshipint = 5;
            if (Content.Split(' ')[1] == "beta") relationshipint = 6;
            if (relationshipint == 0)
            {
                message.Channel.SendMessageAsync("Invalid relationship. Valid are: enemy, neutral, ally, friendly, tribemember, beta");
                return;
            }
            string name = message.Content.Split(' ')[2];
            for (int A = 3; A < message.Content.Split(' ').Length; A++)
            {
                name += " " + message.Content.Split(' ')[A];
            }
            name = Compfort.RemoveChars(name, Constants.InvalidTribenameCharacters);
            if (name.Length > Constants.MaxTribeNameLength)
            {
                message.Channel.SendMessageAsync("Tribename is too long, it can not be longer than " + Constants.MaxTribeNameLength + " characters");
                return;
            }
            if (name.Length < Constants.MinTribeNameLength)
            {
                message.Channel.SendMessageAsync("Tribename is too short, it must be at least " + Constants.MinTribeNameLength + " characters long(perhabs invalid characters were filtered out)");
                return;
            }
            List<TribeData> knowntribes = DataBase.GetTribes(guildid);
            foreach (TribeData tribe in knowntribes)
            {
                if (tribe.name.ToLower().CompareTo(name.ToLower()) == 0)
                {
                    message.Channel.SendMessageAsync("A tribe with this name exists already.");
                    return;
                }
            }
            DataBase.CreateTribe(guildid, name, Compfort.IntToRelationship(relationshipint));
            message.Channel.SendMessageAsync("Tribe " + name + " succesfully created");
        }
        private void DeactivateCommand(SocketMessage message, ulong guildid, string Content)
        {
            IGuildUser user = message.Author as IGuildUser;
            if (message.Author.IsBot)
            {
                message.Channel.SendMessageAsync("Permission issue, as protection the bot can not execute this command. You will need to copy the command the bot posted and execute it yourself");
                return;
            }
            if (user.GuildPermissions.ManageGuild == false)
            {
                message.Channel.SendMessageAsync("You are not permitted to do changes. You need the permission to manage the server in order to use this command.");
                return;
            }
            if (Content.Split(' ').Length > 1)
            {
                bool known = false;
                if (Content.Split(' ')[1] == "steamalert")
                {
                    known = true;
                    if (Content.Split(' ').Length == 4)
                    {
                        Relationship relationship = Compfort.ExtractRelationship(Content.Split(' ')[3]);
                        if (relationship == Relationship.invalid)
                        {
                            message.Channel.SendMessageAsync("Invalid relationship. Valid are: unknown, enemy, neutral, ally, friendly, tribemember, beta");
                            return;
                        }
                        List<ServerData> sData = steamIDControll.FindServer(Content.Split(' ')[2]);//, DataBase.GetGameMode(guildid));
                        if (sData.Count() == 0)
                        {
                            message.Channel.SendMessageAsync(Constants.servernotfoundmessage);
                            return;
                        }
                        if (sData.Count() != 1)
                        {
                            message.Channel.SendMessageAsync(Compfort.MultipleServersFound(sData, message.Author.Id, message.Channel.Id, "!deactivate steamalert ", " " + Content.Split(' ')[3]));
                            return;
                        }
                        if (steamIDControll.DeleteAlertOutOfCache(sData.First(), relationship, message.Channel.Id))
                        {
                            DataBase.DeleteAlert(guildid,message.Channel.Id, relationship, sData.First());
                            message.Channel.SendMessageAsync("Alert for " + sData.First().Name + " deleted");
                        }
                        else
                        {
                            message.Channel.SendMessageAsync("Alert not found, make sure to execute this command in the channel it was created. Use !listalerts for a list of alerts");
                        }
                    }
                    else
                    {
                        message.Channel.SendMessageAsync("Missing parameter. Please use !deactivate steamalert [servernumber] [relationship]");
                    }
                }
                if (known == false)
                {
                    message.Channel.SendMessageAsync("Unknown option, type \"!deactivate\" for a list of options and make sure you spelled it correctly.");
                }
            }
            else
            {
                message.Channel.SendMessageAsync("Please consider specifing what you want to deactivate. Possibilities are: steamalert");
            }
        }
        private void ActivateCommand(SocketMessage message, ulong guildid, string Content)
        {
            IGuildUser user = message.Author as IGuildUser;
            if (message.Author.IsBot)
            {
                message.Channel.SendMessageAsync("Permission issue, as protection the bot can not execute this command. You will need to copy the command the bot posted and execute it yourself");
                return;
            }
            if (user.GuildPermissions.ManageGuild == false)
            {
                message.Channel.SendMessageAsync("You are not permitted to do changes. You need the permission to manage the server in order to use this command.");
                return;
            }
            if (Content.Split(' ').Length > 1)
            {
                bool known = false;
                if (Content.Split(' ')[1] == "steamalert")
                {
                    known = true;
                    if (Content.Split(' ').Length >= 4)
                    {
                        if (DataBase.GetAlertCount(guildid) >= Constants.ServerRelationshipLimit)
                        {
                            message.Channel.SendMessageAsync("Alert limit of " + Constants.ServerRelationshipLimit + " hit, delete some first before you can add more");
                            return;
                        }
                        Relationship relationship = Compfort.ExtractRelationship(Content.Split(' ')[3]);
                        if (relationship == Relationship.invalid)
                        {
                            message.Channel.SendMessageAsync("Invalid relationship. Valid are: unknown, enemy, neutral, ally, friendly, tribemember, beta");
                            return;
                        }
                        List<ServerData> sData = steamIDControll.FindServer(Content.Split(' ')[2], DataBase.GetGameMode(guildid));
                        if (sData.Count() == 0)
                        {
                            message.Channel.SendMessageAsync(Constants.servernotfoundmessage);
                            return;
                        }
                        string mentioningpart = "";
                        for (int a = 4; a < Content.Split(' ').Length; a++)
                        {
                            mentioningpart += " " + message.Content.Split(' ')[a];
                        }
                        //This line is only for the example database, you probably won't need it
                        mentioningpart = Compfort.RemoveChars(mentioningpart, "\n;");
                        if (sData.Count() != 1)
                        {
                            message.Channel.SendMessageAsync(Compfort.MultipleServersFound(sData, message.Author.Id, message.Channel.Id, "!activate steamalert ", " " + Content.Split(' ')[3] + " " + mentioningpart));
                            return;
                        }
                        if (mentioningpart.Length > Constants.MentioningLengthLimit)
                        {
                            message.Channel.SendMessageAsync("Mentioning part to long");
                            return;
                        }
                        DataBase.CreateAlert(guildid, message.Channel.Id, sData.First(), relationship, mentioningpart);
                        steamIDControll.AddAlertToCache(guildid, message.Channel.Id, sData.First(), relationship, mentioningpart);
                        message.Channel.SendMessageAsync("Alert for " + sData.First().Name + " added. If !scan does not return any data for the server, you will need to wait till the next server restart or steam maintainance to receive alerts(this is a ark/steam issue not of the bot). Based on the way the api is designed, some alerts might be delayed or missing");
                    }
                    else
                    {
                        message.Channel.SendMessageAsync("Missing parameter. Please use !activate steamalert [servernumber] [relationship] [Mentions(optional)]");
                    }
                }
                if (known == false)
                {
                    message.Channel.SendMessageAsync("Unknown option, type \"!activate\" for a list of options and make sure you spelled it correctly.");
                }
            }
            else
            {
                message.Channel.SendMessageAsync("Please consider specifing what you want to activate. Possibilities are: steamalert");
            }
        }
        private void ServerCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Split(' ').Length != 2)
            {
                message.Channel.SendMessageAsync("Command invalid. Use !server [servername]");
                return;
            }
            List<ServerData> sData = steamIDControll.FindServer(Content.Split(' ')[1], DataBase.GetGameMode(guildid));
            if (sData.Count() == 0)
            {
                message.Channel.SendMessageAsync(Constants.servernotfoundmessage);
                return;
            }
            if (sData.Count() != 1)
            {
                message.Channel.SendMessageAsync(Compfort.MultipleServersFound(sData, message.Author.Id, message.Channel.Id, "!server ", ""));
                return;
            }
            string rg = "```fix\n[" + sData.First().Name + "]``````apache\nIP: " + sData.First().IP + "```";
            try
            {
                string[] players = A2S.GetPlayersWithTime(sData.First().IP);
                rg += "```fix\nPlayers: " + players.Length + "/" + sData.First().MaxPlayers + "``````apache\nDays:Houers:Min:Sec\n";
                foreach (string s in players)
                {
                    rg += s + "\n";
                    if (rg.Length > 1850)
                    {
                        rg += "```";
                        message.Channel.SendMessageAsync(rg);
                        rg = "apache\n";
                    }
                }
                rg += "```";
            }
            catch (Exception ex)
            {
                rg += "```apache\nServer did not respond in time. Maybe it is offline or just had a temporary outtage.```";
                Compfort.Log(ex.Message + "\n" + ex.StackTrace, LogType.Crash); ;
            }
            rg += "```css\nTribes:";
            List<TribeData> knowntribes = DataBase.GetTribes(guildid, sData.First());
            if (knowntribes.Count > 0)
            {
                foreach (TribeData tribe in knowntribes)
                {
                    string line = "\n-" + tribe.name + " (" + Compfort.RelationshipToString(tribe.relation) + ")";
                    if (rg.Length + line.Length > 1980)
                    {
                        rg += "```";
                        message.Channel.SendMessageAsync(rg);
                        rg = "css\n";
                    }
                    rg += line;
                }
            }
            else
            {
                rg += "\n-no tribe assigned";
            }
            rg += "```";
            if (rg.Length > 10) message.Channel.SendMessageAsync(rg);
        }
        private void RenameTribeCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Split(' ').Length < 3)
            {
                message.Channel.SendMessageAsync("Command invalid. Use !renametribe [old Tribename] [new Tribename)]");
                return;
            }
            string oldtribename = "";
            string newtribename = "";
            oldtribename = message.Content.Split(' ')[1];
            for (int a = 2; a < Content.Split(' ').Length; a++)
            {
                oldtribename += " " + message.Content.Split(' ')[a];
            }
            oldtribename = Compfort.RemoveChars(oldtribename, Constants.InvalidTribenameCharacters);
            List<TribeData> knowntribes = DataBase.GetTribes(guildid);
            TribeData matched = null;
            foreach (TribeData tribe in knowntribes)
            {
                if (matched == null && oldtribename.ToLower().StartsWith(tribe.name.ToLower()))
                {
                    if (tribe.name.Length < oldtribename.Length)
                    {
                        newtribename = oldtribename.Substring(tribe.name.Length + 1);
                    }
                    oldtribename = tribe.name;
                    matched = tribe;
                }
            }
            if (matched == null)
            {
                message.Channel.SendMessageAsync("Tribe can not be found, try checking your spelling and make sure it exists with \"!listalerts\"");
                return;
            }
            if (newtribename.Length > Constants.MaxTribeNameLength)
            {
                message.Channel.SendMessageAsync("Name too long. Limited to " + Constants.MaxTribeNameLength + " characters");
                return;
            }
            if (newtribename.Length < Constants.MinTribeNameLength)
            {
                message.Channel.SendMessageAsync("Name too short. Must be at least " + Constants.MinTribeNameLength + " characters long");
                return;
            }
            if (Compfort.TribeExists(knowntribes, newtribename))
            {
                message.Channel.SendMessageAsync("That tribe already exists");
                return;
            }
            DataBase.RenameTribe(guildid, matched, newtribename);
            message.Channel.SendMessageAsync("\"" + oldtribename + "\" has been renamed to \"" + newtribename + "\"");
        }
        private void SetRelationshipCommand(SocketMessage message, ulong guildid, string Content, bool allrelationships)
        {
            if (Content.Split(' ').Length < 3)
            {
                message.Channel.SendMessageAsync("Command invalid. Use " + Content.Split(' ')[0] + " [Relationship] [Tribename]");
                return;
            }
            int relationship = -1;
            if (Content.Split(' ')[1] == "enemy") relationship = 1;
            if (Content.Split(' ')[1] == "neutral") relationship = 2;
            if (Content.Split(' ')[1] == "allied" || Content.Split(' ')[1] == "ally") relationship = 3;
            if (Content.Split(' ')[1] == "friendly") relationship = 4;
            if (Content.Split(' ')[1] == "tribe" || Content.Split(' ')[1] == "tribemember") relationship = 5;
            if (Content.Split(' ')[1] == "beta") relationship = 6;
            if (relationship == -1)
            {
                message.Channel.SendMessageAsync("Invalid relationship. Valid are: enemy, neutral, ally, friendly, tribemember, beta");
                return;
            }
            string tribename = Content.Split(' ')[2];
            for (int a = 3; a < Content.Split(' ').Length; a++)
            {
                tribename += " " + Content.Split(' ')[a];
            }
            List<TribeData> knowntribes = DataBase.GetTribes(guildid);
            if (knowntribes.Count == 0)
            {
                message.Channel.SendMessageAsync("No tribes have been added yet. Use !createtribe first");
                return;
            }
            foreach (TribeData tribe in knowntribes)
            {
                if (tribe.name.ToLower().CompareTo(tribename) == 0)
                {
                    DataBase.SetRelationship(guildid, tribe, Compfort.IntToRelationship(relationship));
                    if (allrelationships)
                    {
                        List<SteamIDData> SteamIDs = DataBase.GetSteamIDs(guildid, tribe);
                        int updatedids = 0;
                        foreach (SteamIDData steamid in SteamIDs)
                        {
                            DataBase.SetRelationship(guildid, steamid, Compfort.IntToRelationship(relationship));
                            updatedids++;
                        }
                        message.Channel.SendMessageAsync("Changed the relationship of **" + tribe.name + "** to **" + Compfort.RelationshipToString(relationship) + "** aswell as " + updatedids + " SteamIDs");
                        return;
                    }
                    message.Channel.SendMessageAsync("Changed the relationship of **" + tribe.name + "** to **" + Compfort.RelationshipToString(relationship) + "**. All players assigned a relationship manually still have the old relationship use \"!setallrelationships\" to update those aswell");
                    return;
                }
            }
            message.Channel.SendMessageAsync(Constants.unknowntribemessage);
            return;
        }
        private void SetGamemodeCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Split(' ').Length != 2)
            {
                message.Channel.SendMessageAsync("Command is !setgamemode [all, core,classic,conquest,small, crossark]");
                return;
            }
            string gamemode = Content.Split(' ')[1];
            if (gamemode.CompareTo("core") != 0 && gamemode.CompareTo("classic") != 0 && gamemode.CompareTo("conquest") != 0 && gamemode.CompareTo("small") != 0 && !gamemode.Contains("crossark") && gamemode.CompareTo("all") != 0)
            {
                message.Channel.SendMessageAsync("Invalid gamemode options are: all, core, classic, conquest, small, crossark");
                return;
            }
            if (gamemode.Contains("crossark") && gamemode.Length > 10)
            {
                message.Channel.SendMessageAsync("Invalid crossark. If it is a primplus crossark, just use e.g. crossark4");
                return;
            }
            if (gamemode.CompareTo("all") == 0)
            {
                DataBase.SetGamemode(guildid, "");
            }
            else
            {
                DataBase.SetGamemode(guildid, gamemode);
            }
            message.Channel.SendMessageAsync("Gamemode set");
            return;
        }
        private void ToggleLeavelogCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Split(' ').Length != 1)
            {
                message.Channel.SendMessageAsync("Command is just !toggleleavelog without any parameters");
                return;
            }
            if (DataBase.GetLeaveLogStatus(guildid))
            {
                DataBase.ToggleLeaveLog(guildid);
                message.Channel.SendMessageAsync("No longer posting leave messages");
            }
            else
            {
                DataBase.ToggleLeaveLog(guildid);
                message.Channel.SendMessageAsync("From now on posting a message when someone leaves the server");
            }
        }
        private void SetAlertOutputCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Split(' ').Length != 2)
            {
                message.Channel.SendMessageAsync("Command is !setalertoutput [short,detailed]");
                return;
            }
            string alertoutput = Content.Split(' ')[1];
            if (alertoutput.CompareTo("short") != 0 && alertoutput.CompareTo("detailed") != 0)
            {
                message.Channel.SendMessageAsync("Invalid gamemode options are: short,detailed");
                return;
            }
            DataBase.SetAlertOutput(guildid, alertoutput);
            message.Channel.SendMessageAsync("Alertoutput set");
        }
        private void SetSteamIDCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Contains("  "))
            {
                message.Channel.SendMessageAsync("Warning: you have two spaces following each other in the message, this may mess up the command");
            }
            if (Content.Split(' ').Length < 3)
            {
                message.Channel.SendMessageAsync("Command invalid. Use !setsteamid [SteamID] [Relationship(optional)] [Tribe(optional)] [Name(optional)]");
                return;
            }
            if (!Compfort.IsSteamID(Content.Split(' ')[1]))
            {
                message.Channel.SendMessageAsync("SteamID is invalid, Steamids have 17 charaters and start with 7656... . Try steamid.io if you are having issue finding it");
                return;
            }
            ulong steamid = ulong.Parse(Content.Split(' ')[1]);
            if (Content.Split(' ')[1].CompareTo("76561197960287930") == 0)
            {
                message.Channel.SendMessageAsync("SteamID is invalid, this is the example steamid from steamid.io");
                return;
            }
            Relationship relationship = Compfort.ExtractRelationship(Content.Split(' ')[2]);
            if (relationship.CompareTo(Relationship.unknown)==0) relationship = Relationship.invalid;
            int namestartpos = 2;
            if (relationship.CompareTo(Relationship.invalid)!=0) namestartpos = 3;
            TribeData newtribe = null;
            string name = "";
            if (namestartpos < Content.Split(' ').Length)
            {
                string tribename = message.Content.Split(' ')[namestartpos];
                for (int a = namestartpos + 1; a < Content.Split(' ').Length; a++)
                {
                    tribename += " " + message.Content.Split(' ')[a];
                }
                bool matched = false;
                List<TribeData> knowntribes = DataBase.GetTribes(guildid);
                foreach (TribeData tribe in knowntribes)
                {
                    if (matched == false && tribename.ToLower().StartsWith(tribe.name.ToLower()))
                    {
                        if (tribe.name.Length < tribename.Length)
                        {
                            name = tribename.Substring(tribe.name.Length + 1);
                        }
                        else
                        {
                            name = tribename.Substring(tribe.name.Length);
                        }
                        newtribe = tribe;
                        Console.WriteLine("tribe found");
                        matched = true;
                    }
                }
                if (matched == false)
                {
                    name = tribename;
                    tribename = "";
                }
            }
            if (name.Length > Constants.CustomPlayernameMaxLengthLimit)
            {
                message.Channel.SendMessageAsync("Name too long. Limited to " + Constants.CustomPlayernameMaxLengthLimit + " Characters");
                return;
            }
            if (name != "" && name.Length < Constants.CustomPlayernameMinLengthLimit)
            {
                message.Channel.SendMessageAsync("Name too short. Must be at least " + Constants.CustomPlayernameMinLengthLimit + " Characters long");
                return;
            }
            SteamIDData steamiddata = DataBase.GetSteamID(guildid, steamid);
            string rg = "```css\n[Steamid " + steamid + " updated]```";
            if (newtribe != null)
            {
                rg += "```fix\n";
                if (steamiddata.HasTribe()) rg += "old tribe: " + steamiddata.tribe.name + "\n";
                DataBase.SetTribe(guildid, steamiddata, newtribe);
                rg += "new tribe: " + newtribe.name + "```";
            }
            else
            {
                if (steamiddata.HasTribe()) rg += "```fix\ntribe: " + steamiddata.tribe.name + "```";
            }
            if (name != "")
            {
                rg += "```fix\n";
                if (steamiddata.HasName()) rg += "old name: " + steamiddata.name + "\n";
                DataBase.SetName(guildid, steamiddata, name);
                rg += "new name: " + name + "```";
            }
            else
            {
                if (steamiddata.HasName()) rg += "```fix\nname: " + steamiddata.name + "```";
            }
            if (relationship != Relationship.invalid)
            {
                rg += "```fix\n";
                if (steamiddata.HasRelation()) rg += "old relationship: " + Compfort.RelationshipToString(steamiddata.relation) + "\n";
                DataBase.SetRelationship(guildid, steamiddata, relationship);
                rg += "new relationship: " + Compfort.RelationshipToString(relationship) + "```";
            }
            else
            {
                if (steamiddata.HasRelation()) rg += "```fix\nrelationship: " + Compfort.RelationshipToString(steamiddata.relation) + "```";
            }
            message.Channel.SendMessageAsync(rg);
        }
        private void SteamIDDeleteTribeCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Split(' ').Length != 2)
            {
                message.Channel.SendMessageAsync("Command invalid. Use " + Content.Split(' ')[0] + " [SteamID]");
                return;
            }
            if (!Compfort.IsSteamID(Content.Split(' ')[1]))
            {
                message.Channel.SendMessageAsync("SteamID is invalid, Steamids have 17 charaters and start with 7656... . Try steamid.io if you are having issue finding it");
                return;
            }
            ulong steamid = ulong.Parse(Content.Split(' ')[1]);
            DataBase.SetTribe(guildid, DataBase.GetSteamID(guildid, steamid), null);
            message.Channel.SendMessageAsync("The data has been deleted");
        }
        private void SteamIDDeleteRelationshipCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Split(' ').Length != 2)
            {
                message.Channel.SendMessageAsync("Command invalid. Use " + Content.Split(' ')[0] + " [SteamID]");
                return;
            }
            if (!Compfort.IsSteamID(Content.Split(' ')[1]))
            {
                message.Channel.SendMessageAsync("SteamID is invalid, Steamids have 17 charaters and start with 7656... . Try steamid.io if you are having issue finding it");
                return;
            }
            ulong steamid = ulong.Parse(Content.Split(' ')[1]);
            DataBase.SetRelationship(guildid, DataBase.GetSteamID(guildid, steamid), Relationship.unknown);
            message.Channel.SendMessageAsync("The data has been deleted");
        }
        private void SteamIDDeleteNameCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Split(' ').Length != 2)
            {
                message.Channel.SendMessageAsync("Command invalid. Use " + Content.Split(' ')[0] + " [SteamID]");
                return;
            }
            if (!Compfort.IsSteamID(Content.Split(' ')[1]))
            {
                message.Channel.SendMessageAsync("SteamID is invalid, Steamids have 17 charaters and start with 7656... . Try steamid.io if you are having issue finding it");
                return;
            }
            ulong steamid = ulong.Parse(Content.Split(' ')[1]);
            DataBase.SetName(guildid, DataBase.GetSteamID(guildid, steamid), "");
            message.Channel.SendMessageAsync("The data has been deleted");
        }
        private void SteamIDDeleteCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Split(' ').Length != 2)
            {
                message.Channel.SendMessageAsync("Command invalid. Use " + Content.Split(' ')[0] + " [SteamID]");
                return;
            }
            if (!Compfort.IsSteamID(Content.Split(' ')[1]))
            {
                message.Channel.SendMessageAsync("SteamID is invalid, Steamids have 17 charaters and start with 7656... . Try steamid.io if you are having issue finding it");
                return;
            }
            ulong steamid = ulong.Parse(Content.Split(' ')[1]);
            SteamIDData steamiddata = DataBase.GetSteamID(guildid, steamid);
            DataBase.SetName(guildid, steamiddata, "");
            DataBase.SetRelationship(guildid, steamiddata, Relationship.unknown);
            DataBase.SetTribe(guildid, steamiddata, null);
            message.Channel.SendMessageAsync("The data has been deleted");
        }
        private void ListTribesCommand(SocketMessage message, ulong guildid, string Content)
        {
            string rg = "```css\n[List of all tribes]``````diff";
            List<TribeData> knowntribes = DataBase.GetTribes(guildid);
            if (knowntribes.Count == 0)
            {
                message.Channel.SendMessageAsync("No tribes have been added to this discord. Use \"!createtribe\" first");
                return;
            }
            foreach (TribeData tribe in knowntribes)
            {
                string line = "\n";
                if (tribe.relation.CompareTo(Relationship.enemy) == 0) line += "-";
                if (tribe.relation.CompareTo(Relationship.neutral) == 0) line += "#";
                //All other relations are normally more friendly so we just print them out in green
                if (tribe.relation.CompareTo(Relationship.enemy) != 0 && tribe.relation.CompareTo(Relationship.neutral) != 0) line += "+";
                line += " " + tribe.name + " (" + Compfort.RelationshipToString(tribe.relation) + ")";
                List<ServerData> servers = DataBase.GetAllServers(guildid, tribe);
                if (servers.Count > 0)
                {
                    line += " - " + servers.Count + " server";
                    if (servers.Count > 1) line += "s";
                }
                else
                {
                    line += " - no server";
                }
                if (rg.Length + line.Length > 1990)
                {
                    rg += "```";
                    message.Channel.SendMessageAsync(rg);
                    rg = "```diff";
                }
                rg += line;
            }
            rg += "```";
            if (rg.Length > 15) message.Channel.SendMessageAsync(rg);
            return;
        }
        private void ListAlertsCommand(SocketMessage message, ulong guildid, string Content)
        {
            //Reading alerts out of memory and not out of database
            List<ServerData> servers = steamIDControll.servers;
            string rg = "```css\n[List of all alerts]``````css";
            bool hasalert = false;
            foreach (ServerData sData in servers)
            {
                foreach (AlertData alert in sData.alerts)
                {
                    if (alert.GuildID == guildid)
                    {
                        hasalert = true;
                        string line = "\n-" + sData.Name + " for " + alert.Relationship + " in \"";
                        SocketGuildChannel channel = ((SocketGuildChannel)message.Channel).Guild.GetChannel(alert.ChannelID);
                        if (channel != null)
                        {
                            line += channel.Name + "\"";
                            if (alert.Mentions.Length > 0)
                            {
                                string mentionpart = " mentioning:";
                                if (alert.Mentions.ToLower().Contains("@everyone")) mentionpart += " \"everyone\" ";
                                if (alert.Mentions.ToLower().Contains("@here")) mentionpart += " \"here\" ";
                                foreach (string s in alert.Mentions.Split('&'))
                                {
                                    //Filters out @everyone and @here
                                    if (s.Split('>').Length == 2 && ulong.TryParse(s.Split('>')[0], out ulong mentioningid))
                                    {
                                        SocketRole mentionedrole = ((SocketGuildChannel)message.Channel).Guild.GetRole(mentioningid);
                                        if (mentionedrole == null)
                                        {
                                            //Could update the alert at this point to remove the deletedrole from the mentioning
                                            mentionpart += " \"Deleted Role\" ";
                                        }
                                        else
                                        {
                                            mentionpart += " \"" + mentionedrole.Name + "\" ";
                                        }
                                    }
                                }
                                if (mentionpart.Length > 15)
                                {
                                    line += mentionpart;
                                }
                            }
                            if (rg.Length + line.Length > 1990)
                            {
                                rg += "```";
                                message.Channel.SendMessageAsync(rg);
                                rg = "```css";
                            }
                            rg += line;
                        }
                        else
                        {
                            //Channel was deleted while bot was offline, lets delete the alert for it aswell from the database
                            DataBase.DeleteChannelData(channel.Id);
                        }
                    }
                }
            }
            if (hasalert == false) rg += "\nThis server has no alerts";
            rg += "```";
            if (rg.Length > 15) message.Channel.SendMessageAsync(rg);
        }
        private void ScanCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Split(' ').Length != 2 && Content.Split(' ').Length != 3)
            {
                message.Channel.SendMessageAsync("Command needs to be !scan [servernumber] [filtered(default=false)]");
                return;
            }
            List<ServerData> sData = steamIDControll.FindServer(Content.Split(' ')[1], DataBase.GetGameMode(guildid));
            if (sData.Count() == 0)
            {
                message.Channel.SendMessageAsync(Constants.servernotfoundmessage);
                return;
            }
            if (sData.Count() != 1)
            {
                if (Content.Split(' ').Length == 2) message.Channel.SendMessageAsync(Compfort.MultipleServersFound(sData, message.Author.Id, message.Channel.Id, "!scan ", ""));
                if (Content.Split(' ').Length == 3) message.Channel.SendMessageAsync(Compfort.MultipleServersFound(sData, message.Author.Id, message.Channel.Id, "!scan ", " " + Content.Split(' ')[2]));
                return;
            }
            if (Content.Split(' ').Length == 3)
            {
                if (Content.Split(' ')[2] == "true")
                {
                    steamIDControll.Presentserverdata(guildid, message.Channel.Id, sData.First(), true);
                    return;
                }
                else
                {
                    if (Content.Split(' ')[2] != "false") message.Channel.SendMessageAsync("Cound not tell if it should be filtered or not(options are true and false), so just assuming it shoud not be filtered");
                }
            }
            steamIDControll.Presentserverdata(guildid, message.Channel.Id, sData.First(), false);
        }
        private void DeleteTribeCommand(SocketMessage message, ulong guildid, string Content, bool force)
        {
            if (Content.Split(' ').Length < 2)
            {
                message.Channel.SendMessageAsync("Command needs to be " + Content.Split(' ')[0] + " [Tribename]");
                return;
            }
            string name = message.Content.Split(' ')[1];
            for (int A = 2; A < message.Content.Split(' ').Length; A++)
            {
                name += " " + message.Content.Split(' ')[A];
            }
            List<TribeData> knowntribes = DataBase.GetTribes(guildid);
            if (knowntribes.Count > 0)
            {
                TribeData found = null;
                foreach (TribeData tribe in knowntribes)
                {
                    if (tribe.name.ToLower().CompareTo(name.ToLower()) == 0)
                    {
                        found = tribe;
                    }
                }
                if (found == null)
                {
                    message.Channel.SendMessageAsync(Constants.unknowntribemessage);
                    return;
                }
                List<ServerData> servers = DataBase.GetAllServers(guildid, found);
                if (force == false && servers.Count != 0)
                {
                    message.Channel.SendMessageAsync(name + " still has servers. Use \"!deletetribeforce " + name + "\" to continue");
                    return;
                }
                foreach (ServerData sData in servers)
                {
                    DataBase.RemoveTribeFromServer(guildid, sData, found);
                }
                List<SteamIDData> steamids = DataBase.GetSteamIDs(guildid, found);
                foreach (SteamIDData steamid in steamids)
                {
                    DataBase.SetTribe(guildid, steamid, null);
                }
                DataBase.DeleteTribe(guildid, found);
                message.Channel.SendMessageAsync(name + " is now deleted");
                return;
            }
            else
            {
                message.Channel.SendMessageAsync("No tribes have been created yet. Use \"!createtribe\" first");
                return;
            }
        }
        private void TribeinfoCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (Content.Split(' ').Length < 2)
            {
                message.Channel.SendMessageAsync("Command needs to be !tribeinfo [Tribename]");
                return;
            }
            string name = message.Content.Split(' ')[1];
            for (int A = 2; A < message.Content.Split(' ').Length; A++)
            {
                name += " " + message.Content.Split(' ')[A];
            }
            List<TribeData> knowntribes = DataBase.GetTribes(guildid);
            if (knowntribes.Count > 0)
            {
                foreach (TribeData tribe in knowntribes)
                {
                    if (tribe.name.ToLower().CompareTo(name.ToLower()) == 0)
                    {
                        string rg = "";
                        if (tribe.relation.CompareTo(Relationship.tribemember) != 0)
                        {
                            rg = "```css\n[" + tribe.relation + " tribe " + name + "]``````css\nServers:";
                        }
                        else
                        {
                            rg = "```css\n[Own tribe (" + name + ")]``````css\nServers:";
                        }
                        List<ServerData> servers = DataBase.GetAllServers(guildid, tribe);
                        if (servers.Count > 0)
                        {
                            int num = 1;
                            foreach (ServerData server in servers)
                            {
                                string line = "\n" + num + "." + server.Name;
                                num++;
                                try
                                {
                                    int currentplayers = A2S.GetPlayers(server.IP).Count;
                                    line += " (" + currentplayers + "/" + server.MaxPlayers + ")";
                                }
                                catch (Exception)
                                {
                                    line += " (?/" + server.MaxPlayers + ")";
                                }
                                if (rg.Length + line.Length > 1990)
                                {
                                    rg += "```";
                                    message.Channel.SendMessageAsync(rg);
                                    rg = "```css\n";
                                }
                                rg += line;
                            }
                        }
                        else
                        {
                            rg += "\nnone";
                        }
                        rg += "```";
                        if (rg.Length > 1800)
                        {
                            rg += "```";
                            message.Channel.SendMessageAsync(rg);
                            rg = "";
                        }
                        rg += "```css\nManually added steamids: " + DataBase.GetSteamIDs(guildid, tribe).Count + "```";
                        message.Channel.SendMessageAsync(rg);
                        return;
                    }
                }
                message.Channel.SendMessageAsync(Constants.unknowntribemessage);
                return;
            }
            else
            {
                message.Channel.SendMessageAsync("No tribes have been created yet. Use \"!createtribe\" first");
                return;
            }
        }
        private void SteamIDImportCommand(SocketMessage message, ulong guildid, string Content)
        {
            IGuildUser user = message.Author as IGuildUser;
            if (user.GuildPermissions.ManageGuild == false)
            {
                message.Channel.SendMessageAsync("This is sensible data, due to that the command has limited access. You need the permission to manage the server in order to use this command.");
                return;
            }
            if (message.Attachments.Count != 1)
            {
                message.Channel.SendMessageAsync("You need to attach a file gained by using !steamidexport. Caretaker and Haxbot both support this.");
                return;
            }
            if (message.Attachments.First().Filename.Split('.').Length != 2 && message.Attachments.First().Filename.Split('.')[1] != "csv")
            {
                message.Channel.SendMessageAsync("You need to attach a file gained by using !steamidexport. Caretaker and Haxbot both support this. This file is not in a supported format.");
                return;
            }
            WebClient wclient = new WebClient();
            string file = wclient.DownloadString(message.Attachments.First().Url);
            string[] steamdata = file.Split('\n');
            int succes = 0;
            int failed = 0;
            int ignoredauto = 0;
            string Log = "";
            for (int a = 1; a < steamdata.Length; a++)
            {
                string line = "";
                if (steamdata[a].Split(';').Length >= 8)
                {
                    string[] elements = steamdata[a].Split(';');
                    for (int b = 0; b < elements.Length; b++)
                    {
                        elements[b] = Compfort.RemoveChars(elements[b], "\"\\/()[].,;");
                    }
                    if (elements[8].ToLower() == "manual" || elements[8].ToLower() == "\"manual\"")
                    {
                        string steamid = elements[0];
                        if (Compfort.IsSteamID(steamid))
                        {
                            string name = elements[2];
                            Relationship relationship = Compfort.ExtractRelationship(elements[7]);
                            string tribename = elements[6];
                            if (name != "" || relationship.CompareTo(Relationship.invalid) != 0 || tribename != "")
                            {
                                SteamIDData steamiddata = DataBase.GetSteamID(guildid, ulong.Parse(steamid));
                                if (name != "")
                                {
                                    if (steamiddata.HasName())
                                    {
                                        if (steamiddata.name.CompareTo(name) != 0) line += "```css\nChanged name for " + steamid + " from \"" + steamiddata.name + "\" to \"" + name + "\"```";
                                    }
                                    DataBase.SetName(guildid, steamiddata, name);
                                }
                                if (relationship.CompareTo(Relationship.invalid) != 0)
                                {
                                    if (steamiddata.relation.CompareTo(Relationship.invalid)!=0)
                                    {
                                        if (steamiddata.relation.CompareTo(relationship)!=0) line += "```css\nChanged relationship for " + steamid + " from \"" + steamiddata.relation + "\" to \"" + relationship + "\"```";
                                    }
                                    DataBase.SetRelationship(guildid, steamiddata, relationship);
                                }
                                if (tribename != "")
                                {
                                    List<TribeData> knowntribes = DataBase.GetTribes(guildid);
                                    TribeData found = null;
                                    foreach(TribeData tribe in knowntribes) { 
                                        if (tribe.name.ToLower().CompareTo(tribename.ToLower()) == 0)
                                        {
                                            found = tribe;
                                        }
                                    }
                                    if (found == null)
                                    {
                                        if (relationship.CompareTo(Relationship.invalid) != 0)
                                        {
                                            found = DataBase.CreateTribe(guildid, tribename, relationship);
                                            knowntribes.Add(found);
                                            line += "```css\nCreated tribe \"" + tribename + "\" with relationship " + relationship + "```";
                                        }
                                        else
                                        {
                                            line += "```css\nSkipped creation of \"" + tribename + "\" and adding for " + steamid + " due to a missing relationship for the tribe. Maybe it will be added later, id is still skipped, need to rerun !steamidimport command```";
                                        }
                                    }
                                    if (found != null)
                                    {
                                        if (steamiddata.tribe != null)
                                        {
                                            if (steamiddata.tribe.ID != found.ID) line += "```css\nChanged tribe for " + steamid + " from \"" + steamiddata.tribe.name + "\" to \"" + tribename + "\"```";
                                        }
                                        DataBase.SetTribe(guildid, steamiddata, found);
                                    }
                                }
                            }
                            else
                            {
                                line = "```css\nLine " + (a + 1) + " only contains a steamid but no other data```";
                            }
                        }
                        else
                        {
                            line = "```css\nFailed loading of line " + (a + 1) + " missing steamid```";
                        }
                        succes++;
                    }
                    else
                    {
                        ignoredauto++;
                    }
                }
                else
                {
                    failed++;
                    line = "```css\nFailed loading of line " + (a + 1) + " due to invalid parameter count```";
                }
                if (Log.Length + line.Length > 2000)
                {
                    message.Channel.SendMessageAsync(Log);
                    Log = "";
                }
                Log += line;
            }
            if (Log.Length > 1)
            {
                message.Channel.SendMessageAsync(Log);
            }
            message.Channel.SendMessageAsync("Imported: " + succes + "lines\nFailed: " + failed + "lines\nIgnored auto classifications: " + ignoredauto);
        }
        private void SteamIDExportCommand(SocketMessage message, ulong guildid, string Content)
        {
            IGuildUser user = message.Author as IGuildUser;
            if (user.GuildPermissions.ManageGuild == false)
            {
                message.Channel.SendMessageAsync("This is sensible data, due to that the command has limited access. You need the permission to manage the server in order to use this command.");
                return;
            }
            if (Content.Split(' ').Length < 2)
            {
                message.Channel.SendMessageAsync("Command needs to be !steamidexport [Tribename or all]");
                return;
            }
            string name = message.Content.Split(' ')[1];
            for (int A = 2; A < message.Content.Split(' ').Length; A++)
            {
                name += " " + message.Content.Split(' ')[A];
            }
            string data = "\"Steam-ID\";\"Steamname\";\"Known name\";\"Profile-URL\";\"Avatar-link\";\"Tribe-Id\";\"Tribename\";\"Relation\";\"Classification\";\"Previous names\"";
            List<TribeData> knowntribes = DataBase.GetTribes(guildid);
            if (Content.Split(' ')[1].CompareTo("all") != 0 && knowntribes.Count > 0)
            {
                foreach (TribeData tribe in knowntribes)
                {
                    if (tribe.name.ToLower().CompareTo(name.ToLower()) == 0)
                    {
                        name = tribe.name;
                        List<SteamIDData> steamids = DataBase.GetSteamIDs(guildid, tribe);
                        if (steamids.Count > 0)
                        {
                            foreach (SteamIDData steamid in steamids)
                            {
                                data += "\n\"" + steamid.SteamID + "\";;";
                                if (steamid.name.Length > 0)
                                {
                                    data += "\"" + steamid.name + "\"";
                                }
                                data += ";;;\"" + steamid.tribe.ID + "\";\"" + steamid.tribe.name + "\";";
                                if (steamid.relation.CompareTo(Relationship.invalid) != 0)
                                {
                                    data += "\"" + steamid.relation + "\"";
                                }
                                else
                                {
                                    data += "\"" + steamid.relation + "\"";
                                }
                                data += ";\"manual\";";
                            }
                        }
                        else
                        {
                            message.Channel.SendMessageAsync("No steamids have been manually added. Autoclassified steamids can not be exported.");
                            return;
                        }
                        File.WriteAllText(AppContext.BaseDirectory + "\\Export.csv", data);
                        message.Channel.SendFileAsync(AppContext.BaseDirectory + "\\Export.csv", "Data export of tribe " + name);
                        return;
                    }
                }
                message.Channel.SendMessageAsync(Constants.unknowntribemessage);
                message.Channel.SendMessageAsync("This command does not support exporting by relationship as of right now");
                return;
            }
            else
            {
                if (Content.Split(' ')[1].CompareTo("all") == 0)
                {
                    List<SteamIDData> steamids = DataBase.GetSteamIDs(guildid);
                    if (steamids.Count == 0)
                    {
                        message.Channel.SendMessageAsync("There is no SteamID data in this discord that can be exporeted");
                        return;
                    }
                    foreach (SteamIDData steamid in steamids)
                    {
                        data += "\n\"" + steamid.SteamID + "\";;";
                        if (steamid.name.Length > 0) data += "\"" + steamid.name + "\"";
                        data += ";;;";
                        if (steamid.tribe != null) data += "\"" + steamid.tribe.ID + "\"";
                        data += ";";
                        if (steamid.tribe != null) data += "\"" + steamid.tribe.name + "\"";
                        data += ";";
                        if (steamid.relation.CompareTo(Relationship.invalid) != 0)
                        {
                            data += "\"" + steamid.relation + "\"";
                        }
                        else
                        {
                            if (steamid.tribe != null)
                            {
                                data += "\"" + steamid.tribe.relation + "\"";
                            }
                        }
                        data += ";\"manual\";";
                    }
                    File.WriteAllText(AppContext.BaseDirectory + "\\Export.csv", data);
                    message.Channel.SendFileAsync(AppContext.BaseDirectory + "\\Export.csv", "Data export of tribe " + name);
                }
                else
                {
                    message.Channel.SendMessageAsync("No tribes have been created yet. Use \"!createtribe\" first");
                    return;
                }
            }
        }
        private void MarkBotCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (message.Author.Id != AuthorUserID)
            {
                message.Channel.SendMessageAsync("Only bot author can run this command");
                return;
            }
            if (Content.Split(' ').Length == 2)
            {
                message.Channel.SendMessageAsync("Command invalid. Use !markbot" + " [SteamID]");
                return;
            }
            if (!Compfort.IsSteamID(Content.Split(' ')[1]))
            {
                message.Channel.SendMessageAsync("SteamID is invalid, Steamids have 17 charaters and start with 7656... . Try steamid.io if you are having issue finding it");
                return;
            }
            ulong steamid = ulong.Parse(Content.Split(' ')[1]);
            DataBase.AddBot(steamid);
            steamIDControll.AddBotToCache(steamid);
            message.Channel.SendMessageAsync("The ID has been marked as bot");
        }
        private void LoadServerListCommand(SocketMessage message,ulong guildid,string Content)
        {
            if (message.Author.Id != AuthorUserID)
            {
                message.Channel.SendMessageAsync("Only bot author can run this command");
                return;
            }
            if(Content.Split(' ').Length != 2)
            {
                message.Channel.SendMessageAsync("Command is \"!addserverlist [url]\"\nUrl must be in the format of http://arkdedicated.com/officialservers.ini");
                return;
            }
            try
            {
                WebClient Client = new WebClient();
                string downloadstring = Client.DownloadString(Content.Split(' ')[1]);
                message.Channel.SendMessageAsync("Update of serverlist started");
                steamIDControll.UpdateServerList(message,downloadstring);
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
                message.Channel.SendMessageAsync("An error occured, check if your url is valid");
                return;
            }
        }
        private void CreateServerCommand(SocketMessage message, ulong guildid, string Content)
        {
            if (message.Author.Id != AuthorUserID)
            {
                message.Channel.SendMessageAsync("Only bot author can run this command");
                return;
            }
            if (Content.Split(' ').Length != 2)
            {
                message.Channel.SendMessageAsync("Command is \"!createserver [ip:port]\"");
                return;
            }
            try
            {
                A2S_InfoResponse info = A2S.GetInfo(Content.Split(' ')[1]);
                DataBase.CreateServer(info);
                message.Channel.SendMessageAsync("Server added. Please restart the bot to load it");
            }
            catch (Exception)
            {
                //Server probably returned a broken steamid
                try
                {
                    A2S_InfoResponse info = A2S.GetInfoWithoutSteamID(Content.Split(' ')[1]);
                    DataBase.CreateServer(info);
                    message.Channel.SendMessageAsync("Server added without steamid(can not be used for scanning right now due to corrupted data on server side). Please restart the bot to load it");
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(ex2);
                    message.Channel.SendMessageAsync("An error occured:" + ex2.Message);
                    return;
                }
            }
        }
    }
}