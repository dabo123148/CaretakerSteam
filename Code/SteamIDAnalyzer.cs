using Discord;
using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using System.Diagnostics;

namespace Caretaker
{
    public class SteamIDAnalyzer
    {
        /// <summary>
        /// List of Ids of other steamid tracking bots, every id on this list is not getting tracked, so could also be used as ignore list
        /// </summary>
        private List<ulong> bots = new List<ulong>();
        /// <summary>
        /// List of ark devs, everyone in this list has a visual indicator when their id is shown, that they are a dev
        /// </summary>
        private List<ulong> devs = new List<ulong>();
        //Statistics
        private int SteamIDLookups = 0;
        private int ServerLookUps = 0;
        private int JoinsDuringScan = 0;
        private int LeavesDuringScan = 0;
        private int TotalIdsOnline = 0;
        private int scanrequestsprocessed = 0;
        private Program main;
        private Compfort C;
        public List<ServerData> servers = new List<ServerData>();
        private Stopwatch watch = new Stopwatch();
        public SteamIDAnalyzer(Program pmain) 
        {
            main = pmain;
            C = main.C;
            //Initial Loadup
            bots = DataBase.GetAllBots();
            devs = DataBase.GetAllDevs();
            servers = DataBase.GetAllServers();
            SteamAPI.Init();
        }
        /// <summary>
        /// Deletes an alert out of the cache
        /// </summary>
        /// <param name="sData">Server, must be a reference from servers</param>
        /// <param name="r">Relationship of the alert</param>
        /// <param name="channelid">Channelid the alert is for</param>
        /// <returns>Returns true if it managed to find the alert and delete it, false otherwise</returns>
        public bool DeleteAlertOutOfCache(ServerData sData,Relationship r, ulong channelid)
        {
            foreach (AlertData alert in sData.alerts)
            {
                if (alert.ChannelID == channelid && alert.Relationship.CompareTo(r)==0)
                {
                    sData.alerts.Remove(alert);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Adds an alert to the cache
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="channelid"></param>
        /// <param name="sData"></param>
        /// <param name="r"></param>
        /// <param name="mentioningpart"></param>
        public void AddAlertToCache(ulong GuildID,ulong channelid,ServerData sData, Relationship r,string mentioningpart)
        {
            sData.alerts.Add(new AlertData(GuildID, channelid, r, mentioningpart));
        }
        /// <summary>
        /// Adds a SteamID to the bot list
        /// </summary>
        /// <param name="SteamID"></param>
        public void AddBotToCache(ulong SteamID)
        {
            bots.Add(SteamID);
        }
        public List<ServerData> FindServer(string name,string gamemode="")
        {
            name = name.ToLower();
            List<ServerData> Allservers = new List<ServerData>();
            //Initial filtering by gamemode
            foreach(ServerData server in servers)
            {
                string servername = server.Name;
                if(gamemode != "")
                {
                    if (gamemode == "core" && !servername.ToLower().Contains("small") && !servername.ToLower().Contains("conquest") && !servername.ToLower().Contains("classic") && !servername.ToLower().Contains("crossark"))
                    {
                        Allservers.Add(server);
                    }
                    else
                    {
                        if (servername.ToLower().Contains(gamemode))
                        {
                            Allservers.Add(server);
                        }
                    }
                }
                else
                {
                    Allservers.Add(server);
                }
            }
            int matchescount = 0;
            List<ServerData> matches = new List<ServerData>();
            //A servernumber was used to search -> only looking for servers with that number, without this e.g. looking for server 2 could also return 12, 20,21,22..., which is normally not wanted
            if (int.TryParse(name, out int servernummer))
            {
                foreach (ServerData s in Allservers)
                {
                    if (s.Name.Length >= name.Length && int.TryParse(s.Name[s.Name.Length - name.Length].ToString(), out int result) && !int.TryParse(s.Name[s.Name.Length - name.Length-1].ToString(), out int result2))
                    {
                        if (s.Name.ToLower().Contains(name))
                        {
                            matchescount++;
                            matches.Add(s);
                            if (matchescount > Constants.SearchResultLimit) return matches;
                        }
                    }
                }
            }
            else
            {
                foreach (ServerData s in Allservers)
                {
                    if (s.Name.Length >= name.Length) {
                        //Servername matches the search perfectly -> this is the searched server, can discard all other matches(normally shouldn't be any other matches anyway)
                        if (s.Name.ToLower().CompareTo(name) == 0)
                        {
                            matches = new List<ServerData>();
                            matches.Add(s);
                            return matches;
                        }
                        //Check if search matches servername
                        if (s.Name.ToLower().Contains(name))
                        {
                            matchescount++;
                            matches.Add(s);
                            if (matchescount > Constants.SearchResultLimit) return matches;
                        }
                    }
                }
            }
            return matches;
        }
        public static uint ConvertIpToHex(string dottedIpAddress)
        {
            return BitConverter.ToUInt32(IPAddress.Parse(dottedIpAddress).GetAddressBytes(), 0);
        }
        public async void Presentserverdata(ulong guildid,ulong channelid,ServerData sData,bool filtered)
        {
            scanrequestsprocessed++;
            string rg = "```fix\n";
            if (filtered)
            {
                rg += "Filtered s";
            }
            else
            {
                rg += "S";
            }
            rg += "can of " + sData.Name + "``````diff\n-Steam often doesn't know when people leave or join so this list most likely contains people not ingame and might miss a few people. This is a issue of steam/ark and not of the bot.\n";
            if(filtered == false) rg += "You can filter the results using \"!scan [servername] true\" this shows less offline players.\n";
            rg += "Assumptions are in []\nManual input in ()\n";
            List<String> responsemesages = new List<string>();
            List<ulong> steamids;
            steamids = sData.ConnectedIDs;
            //Filters out all steamnames that are on the server
            if (filtered) steamids = FilterIDs(sData, steamids);
            //If ark servers run for a longer time it can happen that they stop returning steamids, this informs the user of this
            if (steamids.Count == 0 || DateTime.Now.Ticks-sData.LastUpdateTime-TimeSpan.TicksPerSecond*Constants.Serverunresponsiveafter>0) rg += "-Server unresponsive/without Steamids for sometime. The next steam maintance/server update fixes this, till then you can just wait. This is a issue of steam/ark and not of this bot.\n-Using data from when the server was responding\n";
            rg += "Data is " + (((double)DateTime.Now.Ticks - (double)sData.LastUpdateTime) / (double)TimeSpan.TicksPerMinute) + " min old\n";
            rg += "Total Ids(bots filtered out): " + steamids.Count + "\n";
            if (filtered)
            {
                rg += "Filtered " + (sData.ConnectedIDs.Count - steamids.Count) + " out\n";
            }
            List<String>[] linesbyrelation = new List<string>[Constants.RelationshipCount+1];
            for (int a = 0; a <= Constants.RelationshipCount; a++)
            {
                linesbyrelation[a] = new List<string>();
            }
            List<String> sortedlines = new List<string>();
            foreach (ulong id in steamids)
            {
                AutoClassificationData autoclass = CalculateAutoclassification(guildid, id);
                string line = id + " ";
                SteamIDData steamiddata = DataBase.GetSteamID(guildid, id);
                if (steamiddata.HasData())
                {
                    if (steamiddata.relation.CompareTo(Relationship.invalid)!=0)
                    {
                        autoclass.Relationship = steamiddata.relation;
                        line += "(" + autoclass.Relationship + ")\t";
                    }
                    else
                    {
                        if (steamiddata.tribe!=null)
                        {
                            autoclass.Relationship = steamiddata.tribe.relation;
                            line += "(" + autoclass.Relationship + ")\t";
                        }
                        else
                        {
                            line += "[" + autoclass.Relationship + "]\t";
                        }
                    }
                }
                else
                {
                    line += "[" + autoclass.Relationship + "]\t";
                }
                if (Compfort.RelationshipToString(autoclass.Relationship).Length <= 7 && autoclass.Relationship != Relationship.neutral) line += "\t";
                if (Compfort.RelationshipToString(autoclass.Relationship).Length <= 5) line += " ";
                if (Compfort.RelationshipToString(autoclass.Relationship).Length <= 7 && autoclass.Relationship != Relationship.unknown) line += " ";
                line += "(Steam:" + GetSteamName(id) + ") ";
                if (steamiddata.tribe != null)
                {
                    line += "(Tribe:" + steamiddata.tribe.name + ") ";
                }
                else
                {
                    line += "[Tribe:" + autoclass.Tribe + "] ";
                }
                if (autoclass.HomeServer == null)
                {
                    line += "[Home: unknown] ";
                }
                else
                {
                    if (!autoclass.HomeServer.server.Name.Contains("CrossArk"))
                    {
                        line += "[Home:" + autoclass.HomeServer.server.Name.Split('-')[0] + "-" + autoclass.HomeServer.server.Name.Split('-')[autoclass.HomeServer.server.Name.Split('-').Length - 1] + "] ";
                    }
                    else
                    {
                        line += "[Home:" + autoclass.HomeServer.server.Name.Split('-')[autoclass.HomeServer.server.Name.Split('-').Length - 2]  + "-" + autoclass.HomeServer.server.Name.Split('-')[autoclass.HomeServer.server.Name.Split('-').Length - 1] + "] ";
                    }
                }
                if (steamiddata.HasName())
                {
                    line += "(Name:" + steamiddata.name + ") ";
                }
                switch ((int)autoclass.Relationship)
                {
                    case 0:
                        line = "#" + line;
                        break;
                    case 1:
                        line = "-" + line;
                        break;
                    case 2:
                        line = "*** " + line + " ***";
                        break;
                    case 3:
                        line = "+" + line;
                        break;
                    case 4:
                        line = "+" + line;
                        break;
                    case 5:
                        line = "+" + line;
                        break;
                    case 6:
                        line = "+" + line;
                        break;
                }
                linesbyrelation[(int)autoclass.Relationship].Add(line);
            }
            for(int a = 0; a <= Constants.RelationshipCount; a++)
            {
                foreach(string s in linesbyrelation[a])
                {
                    if(rg.Length + s.Length > 1980)
                    {
                        rg += "```";
                        responsemesages.Add(rg);
                        rg = "```diff";
                    }
                    rg += "\n" + s;
                }
            }
            if (rg.Length > 10)
            {
                rg += "```";
                responsemesages.Add(rg);
            }
            try
            {
                //Puts the message in a queue(might cause long wait times for a response if a lot of alerts are produced at the same time), can also use C.postMessages(channelid,responsemesages) to bypass the wait time, but then there is a risk of data loss, with a lot of commands at the same time in the channel
                foreach (String s in responsemesages)
                {
                    C.AddMessage(channelid, s);
                }
            }
            catch (Exception)
            {
                Compfort.Log("Server scanning was unable to send a response");
            }
        }
        public List<ulong> FilterIDs(ServerData sData, List<ulong> steamids)
        {
            string[] playernames = new string[0];
            try
            {
                playernames = A2S.GetPlayerNames(sData.IP);
                for (int a = 0; a < playernames.Length; a++)
                {
                    if (playernames[a].Length > 15) playernames[a] = playernames[a].Substring(0, 15);
                }
            }
            catch (Exception)
            {
                //Filters out exception that happens when server is not responding(e.g crashed)
            }
            List<ulong> rg = new List<ulong>();
            foreach (ulong c in steamids)
            {
                string steamname = GetSteamName(c);
                if (steamname.Length > 15) steamname = steamname.Substring(0, 15);
                if (Compfort.ContainsString(playernames, steamname)) rg.Add(c);
            }
            return rg;
        }
        public List<ulong> GetSteamIDs(ServerData sData)
        {
            return GetSteamIDs(sData, Constants.SteamIDQuerryTimeout);
        }
        public List<ulong> GetSteamIDs(ServerData sData,long timeoutinms,int attempts=0)
        {
            uint hexadecimalip = ConvertIpToHex(sData.IP.Split(':')[0]);
            SteamUser.AdvertiseGame(sData.ServerSteamID, hexadecimalip, ushort.Parse(sData.IP.Split(':')[1]));
            int FriendCount = SteamFriends.GetFriendCountFromSource(sData.ServerSteamID);
            //There was data cached, this deltes the data + restarts the request
            if (FriendCount != 0 && attempts < 3)
            {
                SteamUser.TerminateGameConnection(hexadecimalip, ushort.Parse(sData.IP.Split(':')[1]));
                watch.Stop();
                return GetSteamIDs(sData, timeoutinms, attempts + 1);
            }
            long timeoutticks = DateTime.Now.Ticks + TimeSpan.TicksPerMillisecond * timeoutinms;
            //Waits for the data from the server
            while (FriendCount == 0 && DateTime.Now.Ticks < timeoutticks)
            {
                FriendCount = SteamFriends.GetFriendCountFromSource(sData.ServerSteamID);
            }
            //Reads data, if timeout happened, then FriendCount=0
            List<ulong> rg = new List<ulong>();
            for (int a = 0; a < FriendCount; a++)
            {
                ulong result = SteamFriends.GetFriendFromSourceByIndex(sData.ServerSteamID, a).m_SteamID;
                if (!isbot(result)) rg.Add(result);
            }
            //Nessesary for player leaves
            SteamUser.TerminateGameConnection(hexadecimalip, ushort.Parse(sData.IP.Split(':')[1]));
            return rg;
        }
        private string GenerateLeaveMessage(Relationship relation, ServerData sData, PlayData pData)
        {
            string rg = "```diff\n-" + Compfort.RelationshipToString(relation);
            if (relation.CompareTo(Relationship.tribemember) != 0) rg += " player";
            rg += " left " + sData.Name + " after " + Compfort.ToTimeString(DateTime.Now.Ticks-pData.JoinedServer);
            return rg + "```";
        }
        private string GenerateJoinMessage( Relationship relation, ServerData sData,PlayData pData)
        {
            string rg = "```diff\n+" +Compfort.RelationshipToString(relation);
            if (relation.CompareTo(Relationship.tribemember) != 0) rg += " player";
            rg +=" joined " + sData.Name + " (";            
            if (pData.Joins == 1) rg += "1st Join)";
            if (pData.Joins == 2) rg += "2nd Join)";
            if (pData.Joins > 2) rg += pData.Joins +"th Join)";
            return rg + "```";
        }
        private string GetSteamName(ulong SteamID)
        {
            return SteamFriends.GetFriendPersonaName(new CSteamID(SteamID));
        }
        private string GenerateDetailedAlert(ulong SteamID, ulong GuildID, ServerData sData, AutoClassificationData autoclass, Relationship relation)
        {
            PlayData totalserverhistory = getPlayDatabyName(LoadPlayerHistory(GuildID, SteamID, 10000), sData.Name);
            string rg = GenerateJoinMessage(relation, sData, totalserverhistory);
            rg += PresentSteamIDData(GuildID, SteamID);
            return rg;
        }
        private string GenerateShortAlert(ulong SteamID,ulong GuildID,ServerData sData, AutoClassificationData autoclass, Relationship relation,bool join)
        {
            PlayData totalserverhistory = getPlayDatabyName(LoadPlayerHistory(GuildID, SteamID, 10000), sData.Name);
            string rg = "";
            if (join)
            {
                rg += GenerateJoinMessage(relation, sData, totalserverhistory);
            }
            else
            {
                rg += GenerateLeaveMessage(relation, sData, totalserverhistory);
            }
            if (isdev(SteamID)) rg += "```diff\n-THIS PERSON IS A DEV```";
            string manualtribe = "";
            SteamIDData savedsteamiddata = DataBase.GetSteamID(GuildID, SteamID);
            if (savedsteamiddata.HasName() || savedsteamiddata.tribe != null)
            {
                string manual = "```fix";
                if (savedsteamiddata.HasName())
                {
                    manual += "\nName: " + savedsteamiddata.name;
                }
                if (savedsteamiddata.tribe != null)
                {
                    manual += "\nTribe: " + savedsteamiddata.tribe.name;
                }
                manual += "```";
                if (manual.Length > 15) rg += manual;
            }
            rg += "```css\nSteamID: " + SteamID + "\nSteamname: " + GetSteamName(SteamID) + "\n";
            if (manualtribe != autoclass.Tribe)
            {
                rg += "Tribe guess: " + autoclass.Tribe + "\n";
            }
            if (autoclass.HomeServer == null)
            {
                rg += "Home server: not enough data to calculate\n";
            }
            else
            {
                string tribes = Compfort.CreateTribeString(GuildID, autoclass.HomeServer.server);
                if (tribes != "")
                {
                    rg += "Home server: [" + autoclass.HomeServer.server.Name + "] with " + autoclass.HomeServer.Joins + " joins and a playtime of " + Compfort.ToTimeString(autoclass.HomeServer.CalculatePlayTime()) + " withhin the last 14 days (" + tribes + ")\n";
                }
                else
                {
                    rg += "Home server: [" + autoclass.HomeServer.server.Name + "] with " + autoclass.HomeServer.Joins + " joins and a playtime of " + Compfort.ToTimeString(autoclass.HomeServer.CalculatePlayTime()) + " withhin the last 14 days\n";
                }
            }
            if (totalserverhistory != null && totalserverhistory.Joins != 1)
            {
                PlayData last14daypData = getPlayDatabyName(LoadPlayerHistory(GuildID, SteamID, 14), sData.Name);
                int last14daysjoins = 0;
                if (last14daypData != null) last14daysjoins = last14daypData.Joins;
                rg += "Visited the server before " + totalserverhistory.Joins + " out of which " + last14daysjoins + " were in the last 14 days\n";
            }
            rg += "Use \"!sid " + SteamID + "\" for more detailed info```";
            return rg;
        }
        private async Task GenerateAlerts(ServerData sData, List<ulong> connected)
        {
            try
            {
                await Task.Delay(1);
                List<ulong> Joins = new List<ulong>();
                List<ulong> Leaves = new List<ulong>();
                foreach (ulong SteamID in sData.ConnectedIDs)
                {
                    if (!connected.Contains(SteamID))
                    {
                        //Console.WriteLine(SteamID + " left " + sData.Name);
                        Leaves.Add(SteamID);
                    }
                }
                foreach (ulong SteamID in connected)
                {
                    if (!sData.ConnectedIDs.Contains(SteamID))
                    {
                        //Console.WriteLine(SteamID + " joined " + sData.Name);
                        Joins.Add(SteamID);
                    }
                }
                List<ulong> discordserverids = new List<ulong>();
                foreach (AlertData alert in sData.alerts)
                {
                    if (!discordserverids.Contains(alert.GuildID)) discordserverids.Add(alert.GuildID);
                }
                foreach (ulong SteamID in Joins)
                {
                    JoinsDuringScan++;
                    DataBase.SteamIDHistoryAddEntry(SteamID, "j;" + DateTime.Now.Ticks + ";" + sData.Name);
                    foreach (ulong discordserverid in discordserverids)
                    {
                        AutoClassificationData autoclass = CalculateAutoclassification(discordserverid, SteamID);
                        Relationship relationship = Relationship.unknown;
                        SteamIDData savedsteamiddata = DataBase.GetSteamID(discordserverid, SteamID);
                        //Load manually set relationship
                        if (savedsteamiddata.HasData())
                        {
                            if (savedsteamiddata.HasTribe()) relationship = savedsteamiddata.tribe.relation;
                            if (savedsteamiddata.HasRelation()) relationship = savedsteamiddata.relation;
                        }
                        if (relationship.CompareTo(Relationship.unknown) == 0) relationship = autoclass.Relationship;
                        foreach (AlertData alert in sData.alerts)
                        {
                            if (alert.GuildID == discordserverid && alert.Relationship.CompareTo(relationship) == 0)
                            {
                                string alertstring = alert.Mentions + "\n";
                                if (DataBase.GetAlertOutput(discordserverid).CompareTo("detailed") == 0)
                                {
                                    alertstring += GenerateDetailedAlert(SteamID, discordserverid, sData, autoclass, relationship);
                                }
                                else
                                {
                                    alertstring += GenerateShortAlert(SteamID, discordserverid, sData, autoclass, relationship, true);
                                }
                                try
                                {
                                    C.AddMessage(alert.ChannelID, alertstring);
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                }
                foreach (ulong SteamID in Leaves)
                {
                    LeavesDuringScan++;
                    DataBase.SteamIDHistoryAddEntry(SteamID,"l;" + DateTime.Now.Ticks + ";" + sData.Name);
                    foreach (ulong discordserverid in discordserverids)
                    {
                        if (DataBase.GetLeaveLogStatus(discordserverid))
                        {
                            AutoClassificationData autoclass = CalculateAutoclassification(discordserverid, SteamID);
                            Relationship relationship = Relationship.unknown;
                            SteamIDData savedsteamiddata = DataBase.GetSteamID(discordserverid, SteamID);
                            //Load manually set relationship
                            if (savedsteamiddata.HasData())
                            {
                                if (savedsteamiddata.HasTribe()) relationship = savedsteamiddata.tribe.relation;
                                if (savedsteamiddata.HasRelation()) relationship = savedsteamiddata.relation;
                            }
                            if (relationship.CompareTo(Relationship.unknown) == 0) relationship = autoclass.Relationship;
                            foreach (AlertData alert in sData.alerts)
                            {
                                if (alert.GuildID == discordserverid && alert.Relationship.CompareTo(relationship) == 0)
                                {
                                    string alertstring = "";
                                    alertstring += GenerateShortAlert(SteamID, discordserverid, sData, autoclass, relationship, false);
                                    try
                                    {
                                        C.AddMessage(alert.ChannelID, alertstring);
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
                if (Joins.Count != 0 || Leaves.Count != 0)
                {
                    sData.ConnectedIDs = connected;
                    DataBase.SaveConnectedPlayers(sData, sData.ConnectedIDs);
                }
            }catch(Exception ex)
            {
                Compfort.Log("Crash in Alert generation", LogType.Crash);
                Compfort.Log(ex.Message + "\n" + ex.StackTrace, LogType.Crash); ;
            }
        }
        /// <summary>
        /// Scans all servers in servers list, but response time of servers is fairly random, might be faster then serverscannerlinear on some devices, but updatetimes are fairly chaotic
        /// </summary>
        /// <returns></returns>
        public async Task ServerScannerNew()
        {
            await Task.Delay(1);
            try
            {
                long laststeamidupdate = 0;
                while (true)
                {
                    try
                    {
                        if (laststeamidupdate < DateTime.Now.Ticks - TimeSpan.TicksPerMinute * Constants.UpdateSteamServerSteamIDDelay)
                        {
                            laststeamidupdate = DateTime.Now.Ticks;
                            //Update of steamids from crashed servers
                            foreach (ServerData sData in servers)
                            {
                                if (sData.Crashed)
                                {
                                    try
                                    {
                                        CSteamID currentid = new CSteamID(A2S.GetInfo(sData.IP).SteamID);
                                        if (currentid.m_SteamID != sData.ServerSteamID.m_SteamID) DataBase.SetServerSteamID(sData, currentid.m_SteamID);
                                        sData.ServerSteamID = currentid;
                                    }
                                    catch (Exception ex)
                                    {
                                        //Console.WriteLine("Was unable to get serversteamid for " + sData.Name + ": " + ex.Message);
                                    }
                                }
                            }
                        }
                        int responsive = 0;
                        int unresponsive = 0;
                        int notdisconnecting = 0;
                        JoinsDuringScan = 0;
                        LeavesDuringScan = 0;
                        TotalIdsOnline = 0;
                        scanrequestsprocessed = 0;
                        SteamIDLookups = 0;
                        ServerLookUps = 0;
                        int alertsfor = 0;
                        int alertsforrelationship = 0;
                        watch.Reset();
                        watch.Start();
                        foreach (ServerData sData in servers)
                        {
                            TotalIdsOnline += sData.ConnectedIDs.Count;
                            if (sData.alerts.Count != 0)
                            {
                                alertsfor++;
                                alertsforrelationship += sData.alerts.Count;
                            }
                            uint hexadecimalip = ConvertIpToHex(sData.IP.Split(':')[0]);
                            int FriendCount = SteamFriends.GetFriendCountFromSource(sData.ServerSteamID);
                            if (FriendCount > 1)
                            {
                                if (sData.Crashed)
                                {
                                    sData.Crashed = false;
                                }
                                if (sData.Connected)
                                {
                                    if (DateTime.Now.Ticks - sData.LastUpdateTime - TimeSpan.TicksPerSecond * Constants.Serverunresponsiveafter > 0)
                                    {
                                        notdisconnecting++;
                                    }
                                }
                                else
                                {
                                    sData.LastUpdateTime = DateTime.Now.Ticks;
                                    List<ulong> rg = new List<ulong>();
                                    for (int a = 0; a < FriendCount; a++)
                                    {
                                        ulong result = SteamFriends.GetFriendFromSourceByIndex(sData.ServerSteamID, a).m_SteamID;
                                        if (!isbot(result)) rg.Add(result);
                                    }
                                    GenerateAlerts(sData, rg);
                                }
                                sData.Connected = true;
                                responsive++;
                                try
                                {
                                    SteamUser.AdvertiseGame(sData.ServerSteamID, hexadecimalip, ushort.Parse(sData.IP.Split(':')[1]));
                                    await Task.Delay(25);
                                    SteamUser.TerminateGameConnection(hexadecimalip, ushort.Parse(sData.IP.Split(':')[1]));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("was unable to terminate connection");
                                }
                            }
                            else
                            {
                                sData.Connected = false;
                                if (DateTime.Now.Ticks - sData.LastUpdateTime - TimeSpan.TicksPerSecond * Constants.Serverunresponsiveafter > 0)
                                {
                                    if (sData.Crashed)
                                    {
                                        sData.Crashed = false;
                                    }
                                    try
                                    {
                                        CSteamID currentid = new CSteamID(A2S.GetInfo(sData.IP).SteamID);
                                        if (currentid.m_SteamID != sData.ServerSteamID.m_SteamID) DataBase.SetServerSteamID(sData, currentid.m_SteamID);
                                        sData.ServerSteamID = currentid;
                                    }
                                    catch(Exception ex)
                                    {
                                        //Console.WriteLine("Was unable to get serversteamid for " + sData.Name + ": " + ex.Message);
                                    }
                                    unresponsive++;
                                }
                                SteamUser.AdvertiseGame(sData.ServerSteamID, hexadecimalip, ushort.Parse(sData.IP.Split(':')[1]));
                            }
                            await Task.Delay(75);
                        }
                        watch.Stop();
                        try
                        {
                            string dashboardmessage = "Scan time: " + watch.Elapsed.TotalSeconds + "sec\nJoins during scan: " + JoinsDuringScan + "\nLeaves during scan: " + LeavesDuringScan + "\nCurrently Connected: " + TotalIdsOnline + "\nServers monitored: " + servers.Count + "\nServers that are not responding: " + unresponsive + "\nAlerts for: " + alertsfor + " servers\nAlerts for: " + alertsforrelationship + " relationships\nScans processed: " + scanrequestsprocessed + "\nSteamIds looked up: " + SteamIDLookups + "\nServers looked up: " + ServerLookUps + "\nIt is currently " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "(GMT + 2)";
                            if (unresponsive == servers.Count && servers.Count != 0)
                            {
                                Compfort.Log("API IS BLOCKED. Please restart the bot and login with steamfriends");
                            }
                            else
                            {
                                if (unresponsive > servers.Count / 3)
                                {
                                    Compfort.Log("A lot of servers are currently not responding. This is normal when the bot just started, if it should last, you might wanna investigate the reason for it");
                                }
                            }
                            if (Constants.DebugDashboardMessageChannelID != 0 && Constants.DebugDashboardMessageID != 0)
                            {
                                await ((IMessageChannel)main._client.GetChannel(Constants.DebugDashboardMessageChannelID)).GetMessageAsync(Constants.DebugDashboardMessageID).ContinueWith(x =>
                                {
                                    ((IUserMessage)x.Result).ModifyAsync(f => { f.Content = dashboardmessage; });
                                });
                            }

                        }
                        catch (Exception)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        Compfort.Log(ex.Message + "\n" + ex.StackTrace, LogType.Crash);
                    }
                }
            }
            catch (Exception ex)
            {
                Compfort.Log(ex.Message + "\n" + ex.StackTrace, LogType.Crash);
            }
        }
        /// <summary>
        /// Scans all servers in servers list in a linear order, more controlable then ServerScannerNew, but might be slower on some machines
        /// </summary>
        /// <returns></returns>
        public async Task ServerScannerLinear()
        {
            await Task.Delay(1);
            try
            {
                long laststeamidupdate = 0;
                while (true)
                {
                    if (laststeamidupdate < DateTime.Now.Ticks - TimeSpan.TicksPerMinute * Constants.UpdateSteamServerSteamIDDelay)
                    {
                        laststeamidupdate = DateTime.Now.Ticks;
                        //Update of steamids from crashed servers
                        foreach (ServerData sData in servers)
                        {
                            if (sData.Crashed)
                            {
                                try
                                {
                                    CSteamID currentid = new CSteamID(A2S.GetInfo(sData.IP).SteamID);
                                    if (currentid.m_SteamID != sData.ServerSteamID.m_SteamID) DataBase.SetServerSteamID(sData, currentid.m_SteamID);
                                    sData.ServerSteamID = currentid;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Was unable to get serversteamid for " + sData.Name + ": " + ex.Message);
                                }
                            }
                        }
                    }
                    long startime = DateTime.Now.Ticks;
                    int totalconnections = 0;
                    int unresponsive = 0;
                    int longtimeunresponsive = 0;
                    int alertsfor = 0;
                    int alertsforrelationship = 0;
                    int scanrequestsprocessed = 0;
                    JoinsDuringScan = 0;
                    LeavesDuringScan = 0;
                    totalconnections = 0;
                    SteamIDLookups = 0;
                    ServerLookUps = 0;
                    watch.Reset();
                    foreach (ServerData sData in servers)
                    {
                        try
                        {
                            if (sData.Crashed)
                            {
                                sData.ServerSteamID = new CSteamID(A2S.GetInfo(sData.IP).SteamID);
                            }
                            if (sData.alerts.Count != 0)
                            {
                                alertsfor++;
                                alertsforrelationship += sData.alerts.Count;
                            }
                            List<ulong> connected = GetSteamIDs(sData);
                            totalconnections += connected.Count;
                            if (connected.Count > 2)
                            {
                                sData.Crashed = false;
                                GenerateAlerts(sData, connected);
                            }
                            else
                            {
                                sData.Crashed = true;
                                unresponsive++;
                                if (sData.LastUpdateTime + Constants.Serverunresponsiveafter * TimeSpan.TicksPerSecond < DateTime.Now.Ticks) longtimeunresponsive++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Compfort.Log(ex.Message + "\n" + ex.StackTrace, LogType.Crash);
                        }
                    }
                    try
                    {
                        Compfort.Log("Scan time: " + watch.Elapsed.TotalSeconds, LogType.OnlyConsole);
                        string dashboardmessage = "Processing time: " + ((DateTime.Now.Ticks - startime) / TimeSpan.TicksPerSecond) + "sec\nScan time: " + watch.Elapsed.TotalSeconds + "sec\nJoins during scan: " + JoinsDuringScan + "\nLeaves during scan: " + LeavesDuringScan + "\nCurrently Connected: " + totalconnections + "\nServers monitored: " + servers.Count + "\nServers that did not return steamids this scan: " + unresponsive + "\nServers that did not return returning Steamids for multiple scans: " + longtimeunresponsive + "\nAlerts for: " + alertsfor + " servers\nAlerts for: " + alertsforrelationship + " relationships\nScans processed: " + scanrequestsprocessed + "\nSteamIds looked up: " + SteamIDLookups + "\nServers looked up: " + ServerLookUps + "\nIt is currently " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "(GMT + 2)";
                        if (Constants.DebugDashboardMessageChannelID != 0 && Constants.DebugDashboardMessageID != 0)
                        {
                            await ((IMessageChannel)main._client.GetChannel(Constants.DebugDashboardMessageChannelID)).GetMessageAsync(Constants.DebugDashboardMessageID).ContinueWith(x =>
                            {
                                ((IUserMessage)x.Result).ModifyAsync(f => { f.Content = dashboardmessage; });
                            });
                        }
                    }
                    catch (Exception)
                    {

                    }
                    if (unresponsive == servers.Count && servers.Count!=0)
                    {
                        Compfort.Log("API IS BLOCKED. Please restart the bot and login with steamfriends");
                    }
                    else
                    {
                        if (unresponsive > servers.Count / 3)
                        {
                            Compfort.Log("A lot of servers are currently not responding. This is normal when the bot just started, if it should last, you might wanna investigate the reason for it");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Compfort.Log(ex.Message + "\n" + ex.StackTrace, LogType.Crash);
            }
        }
        public PlayData getPlayDatabyName(List<PlayData> pData,string name)
        {
            foreach (PlayData p in pData)
            {
                if (p.server.Name.CompareTo(name) == 0) return p;
            }
            return null;
        }
        public AutoClassificationData CalculateAutoclassification(ulong GuildID,ulong SteamID)
        {
            AutoClassificationData rg = new AutoClassificationData();
            if (isdev(SteamID))
            {
                rg.Relationship = Relationship.enemy; 
                return rg;
            }
            rg.HomeServer = null;
            rg.Tribe = Constants.tribeunknown;
            List<PlayData> pData = sortbyJoins(LoadPlayerHistory(GuildID, SteamID, 14));
            if (pData.Count > 130)
            {
                Compfort.Log("Possible bot sight : " + SteamID, LogType.BotSight);
            }
            List<TotalPlayData> tData = new List<TotalPlayData>();
            //Need at least 2 joins on a server to get classified as from the server
            if (pData.Count > 0 && pData.First().Joins > 2)
            {
                rg.HomeServer = pData.First();
                //Zuweisen von tribes zu pData
                foreach (PlayData p in pData)
                {
                    List<TribeData> tribesonserver = DataBase.GetTribes(GuildID, p.server);
                    if (tribesonserver.Count>0)
                    {
                        foreach (TribeData tribe in tribesonserver)
                        {
                            bool found = false;
                            foreach(TotalPlayData t in tData)
                            {
                                if (t.TribeID==tribe.ID)
                                {
                                    found = true;
                                    t.Joins += p.Joins;
                                }
                            }
                            if (found == false)
                            {
                                TotalPlayData t = new TotalPlayData()
                                {
                                    TribeID = tribe.ID,
                                    Joins = p.Joins
                                };
                                tData.Add(t);
                            }
                        }
                    }
                    else
                    {
                        if (p.Joins > rg.HomeServer.Joins / 5)
                        {
                            bool found = false;
                            foreach (TotalPlayData t in tData)
                            {
                                if (t.TribeID == -1)
                                {
                                    found = true;
                                    t.Joins += p.Joins;
                                }
                            }
                            if (found == false)
                            {
                                TotalPlayData t = new TotalPlayData() { TribeID = -1, Joins = p.Joins };
                                tData.Add(t);
                            }
                        }
                    }
                }
                TotalPlayData top = new TotalPlayData();
                foreach(TotalPlayData t in tData)
                {
                    if(t.Joins > top.Joins || top.Joins == 0)
                    {
                        top = t;
                    }
                }
                if (top.TribeID == -1)
                {
                    rg.Tribe = Constants.tribeunknown;
                }
                else
                {
                    TribeData tribe = Compfort.GetTribeByID(GuildID, top.TribeID);
                    rg.Tribe = tribe.name;
                    rg.Relationship = tribe.relation;
                }
            }
            return rg;
        }
        public bool isdev(ulong steamid)
        {
            foreach (ulong u in devs)
            {
                if (u == steamid) return true;
            }
            return false;
        }
        public bool isbot(ulong steamid)
        {
            foreach(ulong u in bots)
            {
                if (u == steamid) return true;
            }
            return false;
        }
        public List<PlayData> LoadPlayerHistory(ulong guildid,ulong SteamID,int bisdays,int vondays=0)
        {
            List<PlayData> pData = new List<PlayData>();
            try
            {
                string[] lines = DataBase.SteamIDHistory(SteamID).Split('\n');
                if (lines.Length < 1 || (lines.Length == 1 && lines[0].Length==0))
                {
                    return pData;
                }
                foreach (string line in lines)
                {
                    if (long.Parse(line.Split(';')[1]) > DateTime.Now.Ticks - TimeSpan.TicksPerDay * vondays) return pData;
                    bool found = false;
                    foreach (PlayData p in pData)
                    {
                        if (p.server.Name.CompareTo(line.Split(';')[2]) == 0)
                        {
                            found = true;
                            if (line.Split(';')[0].CompareTo("j") == 0)
                            {
                                p.Online = true;
                                p.JoinedServer = long.Parse(line.Split(';')[1]);
                                p.Joins++;
                            }
                            else
                            {
                                p.Online = false;
                                p.PlayTime += long.Parse(line.Split(';')[1]) - p.JoinedServer;
                            }
                        }
                    }
                    if (found == false && line.Split(';')[0].CompareTo("j") == 0 && long.Parse(line.Split(';')[1]) > DateTime.Now.Ticks - TimeSpan.TicksPerDay * bisdays)
                    {
                        PlayData p = new PlayData();
                        p.Online = true;
                        p.Joins++;
                        p.JoinedServer = long.Parse(line.Split(';')[1]);
                        //This causes issues if a server is deleted but it still has data saved for a steamid
                        p.server = FindServer(line.Split(';')[2]).First();
                        pData.Add(p);
                    }
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                Compfort.Log("Fixed Corrupted ID " + SteamID, LogType.Crash);
                string[] lines = DataBase.SteamIDHistory(SteamID).Split('\n');
                DataBase.SteamIDHistoryRemoveEntries(SteamID, lines.Length);
                foreach (string line in lines)
                {
                    if (line.Split(';').Length >= 3 && FindServer(line.Split(';')[2]).Count == 1)
                    {
                        DataBase.SteamIDHistoryAddEntry(SteamID, line);
                    }
                }
                return LoadPlayerHistory(guildid, SteamID, bisdays, vondays);
            }catch(Exception ex)
            {
                Compfort.Log("Caught unexpected exception \n" + ex.Message + "\n" + ex.StackTrace, LogType.Crash);
            }
            return pData;
        }
        public string PresentSteamIDData(ulong guildid, ulong SteamID, int bisdays=14, int vondays=0)
        {
            string rg = "";
            if (isdev(SteamID))
            {
                rg += "```diff\n-This person is a dev```";
            }
            rg += "```css\nSteamID: " + SteamID + "\nSteamname: " + GetSteamName(SteamID) + "```";
            if (DataBase.SteamIDHistory(SteamID).Length == 0)
            {
                return rg + "```css\nNever seen on a monitored server```";
            }
            if (isbot(SteamID))
            {
                return rg + "```css\nThis person is classified as bot. This means no data is beeing collected for this id and the person is not showing up in alerts.```";
            }
            List<PlayData> pData = LoadPlayerHistory(guildid, SteamID, bisdays,vondays);
            pData = sortbyJoins(pData);
            string onserver = "```css\n[Currently on]:\n";
            if (vondays > 0)
            {
                onserver += "not avalible if you set a timeframe";
            }
            else
            {
                int onserverremaing = 10;
                foreach (PlayData p in pData)
                {
                    if (p.Online && onserverremaing > 0)
                    {
                        onserverremaing--;
                        string tribes = Compfort.CreateTribeString(guildid, p.server);
                        string timestring = "";
                        long Ticks = DateTime.Now.Ticks - p.JoinedServer;
                        timestring = Compfort.ToTimeString(Ticks);
                        if (tribes != "")
                        {
                            onserver += "-[" + p.server.Name + "] since " + timestring + " (" + tribes + ")\n";
                        }
                        else
                        {
                            onserver += "-[" + p.server.Name + "] since " + timestring +"\n";
                        }
                    }
                    else
                    {
                        if(p.Online && onserverremaing == 0)
                        {
                            onserverremaing--;
                            onserver += "-Player is registered on more servers, but result is limited to 10";
                        }
                    }
                }
                if (onserver.Length < 25) onserver += "not on any responding server currently";
            }
            onserver += "```";
            if (pData.Count > 10)
            {
                List<PlayData> newPlayData = new List<PlayData>();
                for (int a = 0; a < 10; a++)
                {
                    PlayData top = new PlayData();
                    foreach (PlayData p in pData)
                    {
                        if (p.Joins > top.Joins) top = p;
                        if (p.Joins== top.Joins && p.CalculatePlayTime() > top.CalculatePlayTime()) top = p;
                    }
                    newPlayData.Add(top);
                    pData.Remove(top);
                }
                pData = newPlayData;
            }
            pData = sortbyJoins(pData);
            SteamIDData steamiddata = DataBase.GetSteamID(guildid, SteamID);
            if (steamiddata.HasData())
            {
                rg += "```fix\nManually added info:\n";
                if(steamiddata.HasName()) rg+= "Name: " + steamiddata.name +"\n";
                if (steamiddata.HasTribe()) rg += "Tribe: " + steamiddata.tribe.name + "\n";
                if (steamiddata.HasRelation()) rg += "Relationship: " + steamiddata.relation + "\n";
                rg += "```";
            }
            AutoClassificationData autoclass = CalculateAutoclassification(guildid, SteamID);
            if(autoclass.HomeServer != null || autoclass.Tribe !=null)
            {
                rg += "```css\nAutomated assumptions:";
                if (autoclass.HomeServer != null)
                {
                    string tribes = Compfort.CreateTribeString(guildid, autoclass.HomeServer.server);
                    if (tribes != "")
                    {
                        rg += "\nHome server: [" + autoclass.HomeServer.server.Name + "] with " + autoclass.HomeServer.Joins + " joins and a playtime of " + Compfort.ToTimeString(autoclass.HomeServer.CalculatePlayTime()) + " withhin the last 14 days ("+tribes +")";
                    }
                    else
                    {
                        rg += "\nHome server: [" + autoclass.HomeServer.server.Name + "] with " + autoclass.HomeServer.Joins + " joins and a playtime of " + Compfort.ToTimeString(autoclass.HomeServer.CalculatePlayTime()) + " withhin the last 14 days";
                    }
                }
                if(autoclass.Tribe != null)
                {
                    rg += "\nTribe: " + autoclass.Tribe;
                }
                rg += "\nRelationship: " + autoclass.Relationship;
                rg += "```";
            }
            if (vondays > 0)
            {
                rg += "```css\nServers visited in the last " + vondays + " to " + bisdays + " days:\n";
            }
            else
            {
                rg += "```css\nServers visited in the last " + bisdays + " days:\n";
            }
            foreach (PlayData p in pData)
            {
                string tribes = Compfort.CreateTribeString(guildid, p.server);
                if (tribes != "")
                {
                    rg += "-" + p.Joins + " times [" + p.server.Name + "] with a playtime of " + Compfort.ToTimeString(p.CalculatePlayTime()) + " (" + tribes + ")\n";
                }
                else
                {
                    rg += "-" + p.Joins + " times [" + p.server.Name + "] with a playtime of " + Compfort.ToTimeString(p.CalculatePlayTime()) + "\n";
                }
            }
            rg += "```";
            rg += onserver;
            if(rg.Length > 1900)
            {
                Compfort.Log("SteamID Data too long\n" + rg);
                rg= "The data for this id is too long, please report to the host of the bot. The ID is " + SteamID;
            }
            return rg;
        }
        public List<PlayData> sortbyJoins(List<PlayData> pData)
        {
            List<PlayData> rg = new List<PlayData>();
            while (pData.Count != 0)
            {
                PlayData top = new PlayData();
                foreach (PlayData p in pData)
                {
                    if (p.Joins > top.Joins || top.Joins == 0) top = p;
                }
                rg.Add(top);
                pData.Remove(top);
            }
            return rg;
        }
        public async Task UpdateServerList(Discord.WebSocket.SocketMessage message,string list)
        {
            await Task.Delay(1);
            string[] Server = list.Split('\n');
            foreach (string s in Server)
            {
                string mainip = s.Split(' ')[0];
                try
                {
                    WebClient Client = new WebClient();
                    string servers = Client.DownloadString("http://api.steampowered.com/ISteamApps/GetServersAtAddress/v1?addr=" + mainip);
                    string[] st = Compfort.ExtractAllvalues(servers, "addr\":\"");
                    foreach (string ip in st)
                    {
                        try
                        {
                            A2S_InfoResponse info = A2S.GetInfo(ip);
                            //Implement here a filter if you only want to have certain servers(e.g. only pvp) -> the fewer servers the faster the server data gets updated
                            Compfort.Log("Addr: " + ip + "  " + info.Name);
                            DataBase.CreateServer(info);
                        }
                        catch (Exception)
                        {
                            //Sometimes servers respond without server steamid => this still adds them
                            try
                            {
                                A2S_InfoResponse info = A2S.GetInfoWithoutSteamID(ip);
                                //Implement here a filter if you only want to have certain servers(e.g. only pvp) -> the fewer servers the faster the server data gets updated
                                Compfort.Log("Addr: " + ip + "  " + info.Name);
                                DataBase.CreateServer(info);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            await message.Channel.SendMessageAsync("Update finished. Restart the bot to load the data");
            Compfort.Log("Update of serverlist has been finished");
        }
    }
}