using System;
using System.Collections.Generic;
using System.IO;
namespace Caretaker
{
    public static class DataBase
    {
        /// <summary>
        /// Deletes all data associated with a guild(discord server)
        /// </summary>
        /// <param name="guildid"></param>
        public static void DeleteGuildData(ulong guildid)
        {
            Directory.Delete(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\", true);
        }
        /// <summary>
        /// Deletes all data associated with a channel e.g. alerts in a channel
        /// </summary>
        /// <param name="channelid"></param>
        public static void DeleteChannelData(ulong channelid)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\")) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Guilds\\");
            string[] guilds = Directory.GetDirectories(AppContext.BaseDirectory + "\\Guilds\\");
            foreach (String guild in guilds)
            {
                if (File.Exists(guild + "\\alerts.txt"))
                {
                    string[] alerts = File.ReadAllText(guild + "\\alerts.txt").Split(',');
                    string newfile = "";
                    bool rewrite = false;
                    foreach (string alert in alerts)
                    {
                        if (ulong.Parse(alert.Split(':')[2]) != channelid)
                        {
                            if (newfile != "") newfile += ",";
                            newfile += alert;
                            rewrite = true;
                        }
                    }
                    if (rewrite)
                    {
                        File.WriteAllText(guild + "\\alerts.txt", newfile);
                    }
                }
            }
        }
        /// <summary>
        /// Returns a string with the choice, or "" if the choice isn't found
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="choiceid"></param>
        /// <returns></returns>
        public static string GetChoice(ulong channelid, ulong userid, int choiceid)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Users\\")) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Users\\");
            if (choiceid > 0 && File.Exists(AppContext.BaseDirectory + "\\Users\\" + userid + ".choice"))
            {
                string choice = File.ReadAllText(AppContext.BaseDirectory + "\\Users\\" + userid + ".choice");
                DeleteChoice(userid);
                if (ulong.Parse(choice.Split('\n')[0]) == channelid && choice.Split('\n').Length < choiceid) return choice.Split('\n')[choiceid];
            }
            return "";
        }
        /// <summary>
        /// Saves the choice options, (e.g. server selection when multiple servers are found), it is advisable to delete when calling this the choices in other channels
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="choices"></param>
        public static void OfferChoice(ulong channelid, ulong userid, List<string> choices)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Users\\")) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Users\\");
            string choicestring = channelid.ToString();
            foreach (string s in choices)
            {
                choicestring += "\n" + s;
            }
            File.WriteAllText(AppContext.BaseDirectory + "\\Users\\" + userid + ".choice", choicestring);
        }
        /// <summary>
        /// Deletes all choices of a user
        /// </summary>
        /// <param name="userid"></param>
        public static void DeleteChoice(ulong userid)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Users\\")) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Users\\");
            File.Delete(AppContext.BaseDirectory + "\\Users\\" + userid + ".choice");
        }
        /// <summary>
        /// Returns the set gamemode for a server, default options are "" (means every gamemode), "core"(all non conquest/small/classic servers), "classic","small","conquest"
        /// </summary>
        /// <param name="guildid"></param>
        /// <returns></returns>
        public static string GetGameMode(ulong guildid)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\"))
            {
                if (File.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\gamemode.txt")) return File.ReadAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\gamemode.txt");
            }
            return "";
        }
        /// <summary>
        /// Deletes an alert, the alert is already getting removed out of the cache at an other location
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="relation"></param>
        /// <param name="sData"></param>
        public static void DeleteAlert(ulong guildid, ulong channelid, Relationship relation, ServerData sData)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\")) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\");
            if (File.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\alerts.txt"))
            {
                string[] alerts = File.ReadAllLines(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\alerts.txt");
                string newfile = "";
                foreach (string alert in alerts)
                {
                    if (ulong.Parse(alert.Split(':')[2]) != channelid && relation.CompareTo(Compfort.IntToRelationship(int.Parse(alert.Split(':')[1]))) == 0)
                    {
                        if (newfile != "") newfile += ",";
                        newfile += alert;
                    }
                }
                if (newfile.Length > 0)
                {
                    File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\alerts.txt", newfile);
                }
                else
                {
                    File.Delete(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\alerts.txt");
                }
            }
        }
        /// <summary>
        /// Returns the amount of alerts in a certain guild(discord server). Ignore this if you do not want to limit it
        /// </summary>
        /// <param name="guildid"></param>
        /// <returns></returns>
        public static int GetAlertCount(ulong guildid)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\")) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\");
            if (File.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\alerts.txt")) return File.ReadAllLines(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\alerts.txt").Length;
            return 0;
        }
        /// <summary>
        /// Marks a steamid as bot, bots aren't getting tracked
        /// </summary>
        /// <param name="steamid"></param>
        public static void AddBot(ulong steamid)
        {
            if (!File.Exists(AppContext.BaseDirectory + "\\bots.txt"))
            {
                File.WriteAllText(AppContext.BaseDirectory + "\\bots.txt", steamid.ToString());
            }
            else
            {
                File.WriteAllText(AppContext.BaseDirectory + "\\bots.txt", File.ReadAllText(AppContext.BaseDirectory + "\\bots.txt") + "\n" + steamid.ToString());
            }
        }
        /// <summary>
        /// Returns a long steam with the complete steamid history
        /// </summary>
        /// <param name="steamid"></param>
        /// <returns></returns>
        public static string SteamIDHistory(ulong steamid)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\SteamIDs\\") && File.Exists(AppContext.BaseDirectory + "\\SteamIDs\\" + steamid + ".txt")) return File.ReadAllText(AppContext.BaseDirectory + "\\SteamIDs\\" + steamid + ".txt");
            return "";
        }
        /// <summary>
        /// Deletes the oldest entrycount entries of a steamids history
        /// </summary>
        /// <param name="steamid"></param>
        /// <param name="entrycount"></param>
        public static void SteamIDHistoryRemoveEntries(ulong steamid, int entrycount)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\SteamIDs\\") && File.Exists(AppContext.BaseDirectory + "\\SteamIDs\\" + steamid + ".txt"))
            {
                string[] entries = File.ReadAllText(AppContext.BaseDirectory + "\\SteamIDs\\" + steamid + ".txt").Split('\n');
                if (entries.Length <= entrycount)
                {
                    File.Delete(AppContext.BaseDirectory + "\\SteamIDs\\" + steamid + ".txt");
                }
                else
                {
                    string newlog = "";
                    for (int a = entrycount; a < entries.Length; a++)
                    {
                        if (newlog != "") newlog += "\n";
                        newlog += entries[a];
                    }
                }
            }
        }
        /// <summary>
        /// Saves the ids that are detected on a server(gets loaded only when bot starts)
        /// </summary>
        /// <param name="sData"></param>
        /// <param name="steamids"></param>
        public static void SaveConnectedPlayers(ServerData sData, List<ulong> steamids)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\Servers\\" + sData.Name))
            {
                string file = "";
                foreach (ulong id in steamids)
                {
                    if (file != "") file += "\n";
                    file += id;
                }
                if (file.Length > 0) File.WriteAllText(AppContext.BaseDirectory + "\\Servers\\" + sData.Name + "\\Connected.txt", file);
            }
        }
        /// <summary>
        /// Loads the ids that are detected on a server(gets loaded only when bot starts)
        /// </summary>
        /// <param name="sData"></param>
        public static List<ulong> LoadConnectedPlayers(ServerData sData)
        {
            List<ulong> rg = new List<ulong>();
            if (Directory.Exists(AppContext.BaseDirectory + "\\Servers\\" + sData.Name) && File.Exists(AppContext.BaseDirectory + "\\Servers\\" + sData.Name + "\\Connected.txt"))
            {
                foreach (string id in File.ReadAllLines(AppContext.BaseDirectory + "\\Servers\\" + sData.Name + "\\Connected.txt"))
                {
                    rg.Add(ulong.Parse(id));
                }
            }
            return rg;
        }
        /// <summary>
        /// Returns all alerts created for a server
        /// </summary>
        /// <param name="sData"></param>
        /// <returns></returns>
        public static List<AlertData> GetAlerts(ServerData sData)
        {
            List<AlertData> rg = new List<AlertData>();
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\")) {
                foreach (string guilddir in Directory.GetDirectories(AppContext.BaseDirectory + "\\Guilds\\")) {
                    if (File.Exists(guilddir+ "\\alerts.txt"))
                    {
                        ulong guildid = ulong.Parse(guilddir.Split("\\/".ToCharArray())[guilddir.Split("\\/".ToCharArray()).Length-1]);
                        foreach (string alert in File.ReadAllLines(guilddir + "\\alerts.txt"))
                        {
                            if (sData.Name.CompareTo(alert.Split(';')[0]) == 0)rg.Add(new AlertData(guildid, ulong.Parse(alert.Split(';')[2]), Compfort.IntToRelationship(int.Parse(alert.Split(';')[1])), alert.Split(';')[3]));

                        }
                    }
                }
            }
            return rg;
        }
        /// <summary>
        /// Returns the steamid of a server
        /// </summary>
        /// <param name="sData"></param>
        /// <returns></returns>
        public static ulong GetServerSteamID(ServerData sData)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\Servers\\" + sData.Name) && File.Exists(AppContext.BaseDirectory + "\\Servers\\" + sData.Name + "\\SteamID.txt"))
            {
                return ulong.Parse(File.ReadAllText(AppContext.BaseDirectory + "\\Servers\\" + sData.Name + "\\SteamID.txt"));
            }
            return A2S.GetInfo(sData.IP).SteamID;
        }

        /// <summary>
        /// Sets the server steamid(used for faster startup)
        /// </summary>
        /// <param name="sData"></param>
        /// <param name="steamid"></param>
        public static void SetServerSteamID(ServerData sData, ulong steamid)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\Servers\\" + sData.Name))
            {
                File.WriteAllText(AppContext.BaseDirectory + "\\Servers\\" + sData.Name + "\\SteamID.txt", steamid.ToString());
            }
        }
        /// <summary>
        /// Adds a new entry at the end of the steamid
        /// </summary>
        /// <param name="steamid"></param>
        /// <param name="entry"></param>
        public static void SteamIDHistoryAddEntry(ulong steamid, string entry)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\SteamIDs\\")) Directory.CreateDirectory(AppContext.BaseDirectory + "\\SteamIDs\\");
            if (File.Exists(AppContext.BaseDirectory + "\\SteamIDs\\" + steamid + ".txt"))
            {
                File.WriteAllText(AppContext.BaseDirectory + "\\SteamIDs\\" + steamid + ".txt", File.ReadAllText(AppContext.BaseDirectory + "\\SteamIDs\\" + steamid + ".txt") + "\n" + entry);
            }
            else
            {
                File.WriteAllText(AppContext.BaseDirectory + "\\SteamIDs\\" + steamid + ".txt", entry);
            }
        }
        public static List<ulong> GetAllBots()
        {
            List<ulong> rg = new List<ulong>();
            if (File.Exists(AppContext.BaseDirectory + "\\bots.txt"))
            {
                foreach (string id in File.ReadAllText(AppContext.BaseDirectory + "\\bots.txt").Split('\n'))
                {
                    rg.Add(ulong.Parse(id));
                }
            }
            return rg;
        }
        public static List<ulong> GetAllDevs()
        {
            List<ulong> rg = new List<ulong>();
            if (File.Exists(AppContext.BaseDirectory + "\\devs.txt"))
            {
                foreach (string id in File.ReadAllText(AppContext.BaseDirectory + "\\devs.txt").Split('\n'))
                {
                    rg.Add(ulong.Parse(id));
                }
            }
            return rg;
        }
        /// <summary>
        /// Gets all tribes from a server
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="sData"></param>
        /// <returns></returns>
        public static List<TribeData> GetTribes(ulong guildid, ServerData sData)
        {
            List<TribeData> rg = new List<TribeData>();
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\"))
            {
                string[] tribes = Directory.GetDirectories(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\");
                foreach (string s in tribes)
                {
                    if (File.Exists(s + "\\servers.txt"))
                    {
                        bool found = false;
                        foreach (string server in File.ReadAllText(s + "\\servers.txt").Split('\n'))
                        {
                            if (server.CompareTo(sData.Name) == 0) found = true;
                        }
                        if (found)
                        {
                            rg.Add(new TribeData()
                            {
                                GuildID = guildid,
                                ID = int.Parse(s.Split("\\/".ToCharArray())[s.Split("\\/".ToCharArray()).Length - 1]),
                                name = File.ReadAllText(s + "\\name.txt"),
                                relation = Compfort.IntToRelationship(int.Parse(File.ReadAllText(s + "\\relation.txt")))
                            });
                        }
                    }
                }
            }
            return rg;
        }
        /// <summary>
        /// Returns all tribes in a guild(discord server)
        /// </summary>
        /// <param name="guildid"></param>
        /// <returns></returns>
        public static List<TribeData> GetTribes(ulong guildid)
        {
            List<TribeData> rg = new List<TribeData>();
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\"))
            {
                string[] tribes = Directory.GetDirectories(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\");
                foreach (string s in tribes)
                {
                    rg.Add(new TribeData()
                    {
                        GuildID = guildid,
                        ID = int.Parse(s.Split("\\/".ToCharArray())[s.Split("\\/".ToCharArray()).Length - 1]),
                        name = File.ReadAllText(s + "\\name.txt"),
                        relation = Compfort.IntToRelationship(int.Parse(File.ReadAllText(s + "\\relation.txt")))
                    });
                }
            }
            return rg;
        }
        /// <summary>
        /// Gets the relationship of a tribe
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="tribeID"></param>
        /// <returns></returns>
        public static Relationship GetRelationship(ulong guildid, int tribeID)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribeID)) {
                return Compfort.IntToRelationship(int.Parse(File.ReadAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribeID + "\\relation.txt")));
            }
            return Relationship.unknown;
        }
        /// <summary>
        /// Returns all servers that are added to the bot
        /// </summary>
        /// <returns></returns>
        public static List<ServerData> GetAllServers()
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Servers\\")) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Servers\\");
            List<ServerData> rg = new List<ServerData>();
            string[] servers = Directory.GetDirectories(AppContext.BaseDirectory + "\\Servers\\");
            foreach (string s in servers)
            {
                rg.Add(new ServerData(s.Split("\\/".ToCharArray())[s.Split("\\/".ToCharArray()).Length - 1], File.ReadAllText(s + "\\Ip.txt"), int.Parse(File.ReadAllText(s + "\\MaxPlayers.txt")), true));
            }
            return rg;
        }
        /// <summary>
        /// Adds a server to the database
        /// </summary>
        /// <param name="data"></param>
        public static void CreateServer(A2S_InfoResponse data)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Servers\\")) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Servers\\");
            Directory.CreateDirectory(AppContext.BaseDirectory + "\\Servers\\" + data.Name);
            File.WriteAllText(AppContext.BaseDirectory + "\\Servers\\" + data.Name + "\\Ip.txt", data.IP);
            File.WriteAllText(AppContext.BaseDirectory + "\\Servers\\" + data.Name + "\\SteamID.txt", data.SteamID.ToString());
            File.WriteAllText(AppContext.BaseDirectory + "\\Servers\\" + data.Name + "\\MaxPlayers.txt", data.MaxPlayers.ToString());
        }
        /// <summary>
        /// Returns all servers a tribe owns, this data does not return the data that is used for alert generation
        /// </summary>
        /// <returns></returns>
        public static List<ServerData> GetAllServers(ulong guildid, TribeData tribe)
        {
            List<ServerData> rg = new List<ServerData>();
            if (File.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID + "\\Servers.txt"))
            {
                string[] servers = File.ReadAllLines(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID + "\\Servers.txt");
                foreach (string servername in servers)
                {
                    rg.Add(new ServerData(servername, File.ReadAllText(AppContext.BaseDirectory + "\\Servers\\" + servername + "\\Ip.txt"), int.Parse(File.ReadAllText(AppContext.BaseDirectory + "\\Servers\\" + servername + "\\MaxPlayers.txt")), true));
                }
            }
            return rg;
        }
        /// <summary>
        /// Deletes a server from the database, not in use anywhere -> you will also need to implement a function to remove the server from the cache
        /// </summary>
        public static void DeleteServer(ServerData sData)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Servers\\")) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Servers\\");
            Directory.Delete(AppContext.BaseDirectory + "\\Servers\\" + sData.Name, false);
        }
        /// <summary>
        /// Creates a new alert, it is loaded into the cache at an other location already
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="channelid"></param>
        /// <param name="sData"></param>
        /// <param name="relation"></param>
        /// <param name="mentioningpart"></param>
        public static void CreateAlert(ulong guildid, ulong channelid, ServerData sData, Relationship relation, string mentioningpart)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\")) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\");
            if (File.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\alerts.txt"))
            {
                foreach (AlertData alert in DataBase.GetAlerts(sData))
                {
                    //Prevents duplicates
                    if (alert.ChannelID == channelid && mentioningpart.CompareTo(alert.Mentions) == 0) return;
                }
                File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\alerts.txt",File.ReadAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\alerts.txt")+"\n"+sData.Name+";"+(int)(relation) + ";"+channelid+";"+mentioningpart);
            }
            else
            {
                File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\alerts.txt", sData.Name + ";" + (int)(relation) + ";" + channelid + ";" + mentioningpart);
            }
        }
        /// <summary>
        /// Saves a tribe and creates a unique tribeid for it(unique identifier, depending on implementation must be fully unique or just withhin a guild(discord server) unique)
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="tribename"></param>
        /// <param name="relation"></param>
        /// <returns>Returns the created tribe</returns>
        public static TribeData CreateTribe(ulong guildid, string tribename, Relationship relation)
        {
            int nextID = 0;
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\");
            }
            else
            {
                nextID = int.Parse(File.ReadAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\nextID.txt"));
            }
            Directory.CreateDirectory(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + nextID);
            File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + nextID + "\\Relation.txt", ((int)relation).ToString());
            File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + nextID + "\\Name.txt", tribename);
            File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\nextID.txt", (nextID + 1).ToString());
            //Still needs ID
            return new TribeData() { GuildID = guildid, name = tribename, relation = relation, ID = nextID };
        }
        /// <summary>
        /// Deletes a tribe, deletion from other sources(steamids, servers) is handled already
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="tribe"></param>
        public static void DeleteTribe(ulong guildid, TribeData tribe)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID))
            {
                if (File.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID + "\\Servers.txt"))
                {
                    string[] servers = File.ReadAllLines(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID + "\\Servers.txt");
                    foreach (string server in servers) {
                        string[] oldfile = File.ReadAllLines(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Servers\\" + server + "\\Tribes.txt");
                        string newfile = "";
                        foreach (string s in oldfile)
                        {
                            if (s.CompareTo(tribe.ID.ToString()) != 0)
                            {
                                if (newfile != "") newfile += "\n";
                                newfile += s;
                            }
                        }
                        if (newfile.Length != 0)
                        {
                            File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Servers\\" + server + "\\Tribes.txt", newfile);
                        }
                        else
                        {
                            Directory.Delete(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Servers\\" + server);
                        }
                    }
                }
                Directory.Delete(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID, true);
            }
        }
        /// <summary>
        /// Adds a tribe to a server
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="sData"></param>
        /// <param name="tribe"></param>
        public static void AddTribeToServer(ulong guildid, ServerData sData, TribeData tribe)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Server\\" + sData.Name))
            {
                string newstring = File.ReadAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Server\\" + sData.Name + "\\Tribes.txt");
                bool found = false;
                foreach (string id in newstring.Split('\n'))
                {
                    if (id.CompareTo(tribe.ID.ToString()) == 0) found = true;
                }
                if (!found)
                {
                    File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Server\\" + sData.Name + "\\Tribes.txt", newstring + "\n" + tribe.ID.ToString());
                }
                else
                {
                    return;
                }
            }
            else
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Server\\" + sData.Name);
                File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Server\\" + sData.Name + "\\Tribes.txt", tribe.ID.ToString());
            }
            if (File.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID + "\\Servers.txt"))
            {
                File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID + "\\Servers.txt", File.ReadAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID + "\\Servers.txt") + "\n" + sData.Name);
            }
            else
            {
                File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID + "\\Servers.txt", sData.Name);
            }
        }
        /// <summary>
        /// Removes a tribe from a server
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="sData"></param>
        /// <param name="tribe"></param>
        public static void RemoveTribeFromServer(ulong guildid, ServerData sData, TribeData tribe)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Server\\" + sData.Name))
            {
                string oldstring = File.ReadAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Server\\" + sData.Name + "\\Tribes.txt");
                string newstring = "";
                foreach (string id in oldstring.Split('\n'))
                {
                    if (id.CompareTo(tribe.ID.ToString()) != 0)
                    {
                        if (newstring != "") newstring += "\n";
                        newstring += id;
                    }
                }
                if (newstring.Length != 0)
                {
                    File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Server\\" + sData.Name + "\\Tribes.txt", newstring);
                }
                else
                {
                    File.Delete(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Server\\" + sData.Name + "\\Tribes.txt");
                    if (Directory.GetFiles(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Server\\" + sData.Name).Length == 0) Directory.Delete(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Server\\" + sData.Name);
                }
                oldstring = File.ReadAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID + "\\Servers.txt");
                newstring = "";
                foreach (string name in oldstring.Split('\n'))
                {
                    if (name.CompareTo(sData.Name) != 0)
                    {
                        if (newstring != "") newstring += "\n";
                        newstring += name;
                    }
                }
                if (newstring.Length != 0)
                {
                    File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID + "\\Servers.txt", newstring);
                }
                else
                {
                    File.Delete(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID + "\\Servers.txt");
                }
            }
        }
        /// <summary>
        /// Renames a tribe
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="tribe"></param>
        /// <param name="newtribename"></param>
        public static void RenameTribe(ulong guildid, TribeData tribe, string newtribename)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID))
            {
                File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID + "\\Name.txt", newtribename);
            }
        }
        /// <summary>
        /// Sets the relationship of a tribe
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="tribe"></param>
        /// <param name="relation"></param>
        public static void SetRelationship(ulong guildid, TribeData tribe, Relationship relation)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID))
            {
                File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\Tribes\\" + tribe.ID + "\\Relation.txt", ((int)relation).ToString());
            }
        }
        /// <summary>
        /// Sets the relationship of a steamid
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="tribe"></param>
        /// <param name="relation"></param>
        public static void SetRelationship(ulong guildid, SteamIDData steamid, Relationship relation)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID)) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID);
            if (relation.CompareTo(Relationship.invalid) != 0 && relation.CompareTo(Relationship.unknown) != 0)
            {
                File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID + "\\Relationship.txt", ((int)relation).ToString());
            }
            else
            {
                File.Delete(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID + "\\Relationship.txt");
                if (Directory.GetFiles(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID).Length == 0) Directory.Delete(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID);
            }
        }

        /// <summary>
        /// Gets all steamids of a tribe
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="tribe"></param>
        /// <returns></returns>
        public static List<SteamIDData> GetSteamIDs(ulong guildid, TribeData tribe)
        {
            List<SteamIDData> rg = new List<SteamIDData>();
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\"))
            {
                foreach (string steamiddir in Directory.GetDirectories(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\"))
                {
                    if(File.Exists(steamiddir + "\\Tribe.txt") && int.Parse(File.ReadAllText(steamiddir + "\\Tribe.txt"))==tribe.ID) rg.Add(DataBase.GetSteamID(guildid, ulong.Parse(steamiddir.Split("\\/".ToCharArray())[steamiddir.Split("\\/".ToCharArray()).Length - 1])));
                }
            }
            return rg;
        }
        /// <summary>
        /// Gets all steamids of a guild(discord server)
        /// </summary>
        /// <param name="guildid"></param>
        /// <returns></returns>
        public static List<SteamIDData> GetSteamIDs(ulong guildid)
        {
            List<SteamIDData> rg = new List<SteamIDData>();
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\")){
                foreach(string steamiddir in Directory.GetDirectories(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\"))
                {
                    rg.Add(DataBase.GetSteamID(guildid, ulong.Parse(steamiddir.Split("\\/".ToCharArray())[steamiddir.Split("\\/".ToCharArray()).Length - 1])));
                }
            }
            return rg;
        }
        /// <summary>
        /// Sets a gamemode for a guild(discord server). Is used for filtering servers
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="gamemode"></param>
        public static void SetGamemode(ulong guildid, string gamemode)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid))
            {
                File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\gamemode.txt", gamemode);
            }
        }
        /// <summary>
        /// Returns if a certain guild(discord server) also has leave alerts enabled
        /// </summary>
        /// <param name="guildid"></param>
        /// <returns></returns>
        public static bool GetLeaveLogStatus(ulong guildid)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid))
            {
                return File.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\leavelog.txt");
            }
            return false;
        }
        /// <summary>
        /// Toggles the leavelog status in a guild(discord server)
        /// </summary>
        /// <param name="guildid"></param>
        public static void ToggleLeaveLog(ulong guildid)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid)) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Guilds\\" + guildid);
            if (File.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\leavelog.txt"))
            {
                File.Delete(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\leavelog.txt");
            }
            else { 
                File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\leavelog.txt", "true");
            }
        }
        /// <summary>
        /// Alert output, default detailed
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="mode"></param>
        public static void SetAlertOutput(ulong guildid, string mode)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid)) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Guilds\\" + guildid);
            File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\alertoutput.txt", mode);
        }
        /// <summary>
        /// Returns the alert output
        /// </summary>
        /// <param name="guildid"></param>
        /// <returns>detailed or short</returns>
        public static string GetAlertOutput(ulong guildid)
        {
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid) && File.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\alertoutput.txt"))
            {
                return File.ReadAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\alertoutput.txt");
            }
            return "detailed";
        }
        /// <summary>
        /// Returns the data for a certain steamid
        /// </summary>
        /// <param name="guilid"></param>
        /// <returns></returns>
        public static SteamIDData GetSteamID(ulong guildid, ulong steamid)
        {
            SteamIDData rg = new SteamIDData { SteamID = steamid };
            if (Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid))
            {
                if (File.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid + "\\Name.txt")) rg.name = File.ReadAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid + "\\Name.txt");
                if (File.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid + "\\Relationship.txt")) rg.relation = Compfort.IntToRelationship(int.Parse(File.ReadAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid + "\\Relationship.txt")));
                if (File.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid + "\\Tribe.txt")) rg.tribe = Compfort.GetTribeByID(guildid,int.Parse(File.ReadAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid + "\\Tribe.txt")));
            }
            return rg;
        }
        /// <summary>
        /// Sets a custom name for a steamid withhin a guild(discord server)
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="steamid"></param>
        /// <param name="name"></param>
        public static void SetName(ulong guildid, SteamIDData steamid, string name)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID)) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID);
            if (name != null && name != "")
            {
                File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID + "\\Name.txt", name);
            }
            else
            {
                File.Delete(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID + "\\Name.txt");
                if (Directory.GetFiles(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID).Length == 0) Directory.Delete(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID);
            }
        }
        /// <summary>
        /// Assoziates a steamid with a tribe withhin a guild(discord server)
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="steamid"></param>
        /// <param name="tribe"></param>
        public static void SetTribe(ulong guildid, SteamIDData steamid, TribeData tribe)
        {
            if (!Directory.Exists(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID)) Directory.CreateDirectory(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID);
            if (tribe != null)
            {
                File.WriteAllText(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID + "\\Tribe.txt", tribe.ID.ToString());
            }
            else
            {
                File.Delete(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID + "\\Tribe.txt");
                if (Directory.GetFiles(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID).Length == 0) Directory.Delete(AppContext.BaseDirectory + "\\Guilds\\" + guildid + "\\SteamIDs\\" + steamid.SteamID);
            }
        }
    }
}