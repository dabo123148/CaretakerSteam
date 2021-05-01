using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caretaker
{
    public static class OriginalDataBase
    {
        /// <summary>
        /// Deletes all data associated with a guild(discord server)
        /// </summary>
        /// <param name="guildid"></param>
        public static void DeleteGuildData(ulong guildid)
        {

        }
        /// <summary>
        /// Deletes all data associated with a channel e.g. alerts in a channel
        /// </summary>
        /// <param name="channelid"></param>
        public static void DeleteChannelData(ulong channelid)
        {

        }
        /// <summary>
        /// Returns a string with the choice, or "" if the choice isn't found
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="choiceid"></param>
        /// <returns></returns>
        public static string GetChoice(ulong channelid, ulong userid, int choiceid)
        {
            return "";
        }
        /// <summary>
        /// Saves the choice options, (e.g. server selection when multiple servers are found), it is advisable to delete when calling this the choices in other channels
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="choices"></param>
        public static void OfferChoice(ulong channelid, ulong userid, List<string> choices)
        {

        }
        /// <summary>
        /// Deletes all choices of a user
        /// </summary>
        /// <param name="userid"></param>
        public static void DeleteChoice(ulong userid)
        {

        }
        /// <summary>
        /// Returns the set gamemode for a server, default options are "" (means every gamemode), "core"(all non conquest/small/classic servers), "classic","small","conquest"
        /// </summary>
        /// <param name="guildid"></param>
        /// <returns></returns>
        public static string GetGameMode(ulong guildid)
        {
            return "";
        }
        /// <summary>
        /// Deletes an alert, the alert is already getting removed out of the cache at an other location
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="relation"></param>
        /// <param name="sData"></param>
        public static void DeleteAlert(ulong guildid,ulong channelid, Relationship relation, ServerData sData)
        {

        }
        /// <summary>
        /// Returns the amount of alerts in a certain guild(discord server). Ignore this if you do not want to limit it
        /// </summary>
        /// <param name="guildid"></param>
        /// <returns></returns>
        public static int GetAlertCount(ulong guildid)
        {
            return 0;
        }
        /// <summary>
        /// Marks a steamid as bot, bots aren't getting tracked
        /// </summary>
        /// <param name="steamid"></param>
        public static void AddBot(ulong steamid)
        {

        }
        /// <summary>
        /// Returns a long steam with the complete steamid history
        /// </summary>
        /// <param name="steamid"></param>
        /// <returns></returns>
        public static string SteamIDHistory(ulong steamid)
        {
            return "";
        }
        /// <summary>
        /// Deletes the oldest entrycount entries of a steamids history
        /// </summary>
        /// <param name="steamid"></param>
        /// <param name="entrycount"></param>
        public static void SteamIDHistoryRemoveEntries(ulong steamid, int entrycount)
        {

        }
        /// <summary>
        /// Saves the ids that are detected on a server(gets loaded only when bot starts)
        /// </summary>
        /// <param name="sData"></param>
        /// <param name="steamids"></param>
        public static void SaveConnectedPlayers(ServerData sData, List<ulong> steamids)
        {

        }
        /// <summary>
        /// Loads the ids that are detected on a server(gets loaded only when bot starts)
        /// </summary>
        /// <param name="sData"></param>
        public static List<ulong> LoadConnectedPlayers(ServerData sData)
        {
            return new List<ulong>();
        }
        /// <summary>
        /// Returns all alerts created for a server
        /// </summary>
        /// <param name="sData"></param>
        /// <returns></returns>
        public static List<AlertData> GetAlerts(ServerData sData)
        {
            return new List<AlertData>();
        }
        /// <summary>
        /// Returns the steamid of a server
        /// </summary>
        /// <param name="sData"></param>
        /// <returns></returns>
        public static ulong GetServerSteamID(ServerData sData)
        {
            return 0;
        }

        /// <summary>
        /// Sets the server steamid(used for faster startup)
        /// </summary>
        /// <param name="sData"></param>
        /// <param name="steamid"></param>
        public static void SetServerSteamID(ServerData sData, ulong steamid)
        {
        }
        /// <summary>
        /// Adds a new entry at the end of the steamid
        /// </summary>
        /// <param name="steamid"></param>
        /// <param name="entry"></param>
        public static void SteamIDHistoryAddEntry(ulong steamid, string entry)
        {

        }
        public static List<ulong> GetAllBots()
        {
            return new List<ulong>();
        }
        public static List<ulong> GetAllDevs()
        {
            return new List<ulong>();
        }
        /// <summary>
        /// Gets all tribes from a server
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="sData"></param>
        /// <returns></returns>
        public static List<TribeData> GetTribes(ulong guildid, ServerData sData)
        {
            return new List<TribeData>();
        }
        /// <summary>
        /// Returns all tribes in a guild(discord server)
        /// </summary>
        /// <param name="guildid"></param>
        /// <returns></returns>
        public static List<TribeData> GetTribes(ulong guildid)
        {
            return new List<TribeData>();
        }
        /// <summary>
        /// Gets the relationship of a tribe
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="tribeID"></param>
        /// <returns></returns>
        public static Relationship GetRelationship(ulong guildid, int tribeID)
        {
            return Relationship.unknown;
        }
        /// <summary>
        /// Returns all servers that are added to the bot
        /// </summary>
        /// <returns></returns>
        public static List<ServerData> GetAllServers()
        {
            return new List<ServerData>();
        }
        /// <summary>
        /// Adds a server to the database
        /// </summary>
        /// <param name="data"></param>
        public static void CreateServer(A2S_InfoResponse data)
        {
        }
        /// <summary>
        /// Returns all servers a tribe owns, this data does not return the data that is used for alert generation
        /// </summary>
        /// <returns></returns>
        public static List<ServerData> GetAllServers(ulong guildid, TribeData tribe)
        {
            return new List<ServerData>();
        }
        /// <summary>
        /// Deletes a server from the database, not in use anywhere -> you will also need to implement a function to remove the server from the cache
        /// </summary>
        public static void DeleteServer(ServerData sData)
        {

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
            //Still needs ID
            return new TribeData() { GuildID = guildid, name = tribename, relation = relation };
        }
        /// <summary>
        /// Deletes a tribe, deletion from other sources(steamids, servers) is handled already
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="tribe"></param>
        public static void DeleteTribe(ulong guildid, TribeData tribe)
        {
        }
        /// <summary>
        /// Adds a tribe to a server
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="sData"></param>
        /// <param name="tribe"></param>
        public static void AddTribeToServer(ulong guildid, ServerData sData, TribeData tribe)
        {

        }
        /// <summary>
        /// Removes a tribe from a server
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="sData"></param>
        /// <param name="tribe"></param>
        public static void RemoveTribeFromServer(ulong guildid, ServerData sData, TribeData tribe)
        {

        }
        /// <summary>
        /// Renames a tribe
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="tribe"></param>
        /// <param name="newtribename"></param>
        public static void RenameTribe(ulong guildid, TribeData tribe, string newtribename)
        {

        }
        /// <summary>
        /// Sets the relationship of a tribe
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="tribe"></param>
        /// <param name="relation"></param>
        public static void SetRelationship(ulong guildid, TribeData tribe, Relationship relation)
        {

        }
        /// <summary>
        /// Sets the relationship of a steamid
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="tribe"></param>
        /// <param name="relation"></param>
        public static void SetRelationship(ulong guildid, SteamIDData steamid, Relationship relation)
        {

        }
        /// <summary>
        /// Gets all steamids of a tribe
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="tribe"></param>
        /// <returns></returns>
        public static List<SteamIDData> GetSteamIDs(ulong guildid, TribeData tribe)
        {
            return new List<SteamIDData>();
        }
        /// <summary>
        /// Gets all steamids of a guild(discord server)
        /// </summary>
        /// <param name="guildid"></param>
        /// <returns></returns>
        public static List<SteamIDData> GetSteamIDs(ulong guildid)
        {
            return new List<SteamIDData>();
        }
        /// <summary>
        /// Sets a gamemode for a guild(discord server). Is used for filtering servers
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="gamemode"></param>
        public static void SetGamemode(ulong guildid, string gamemode)
        {

        }
        /// <summary>
        /// Returns if a certain guild(discord server) also has leave alerts enabled
        /// </summary>
        /// <param name="guildid"></param>
        /// <returns></returns>
        public static bool GetLeaveLogStatus(ulong guildid)
        {
            return false;
        }
        /// <summary>
        /// Toggles the leavelog status in a guild(discord server)
        /// </summary>
        /// <param name="guildid"></param>
        public static void ToggleLeaveLog(ulong guildid)
        {

        }
        /// <summary>
        /// Alert output, default detailed
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="mode"></param>
        public static void SetAlertOutput(ulong guildid, string mode)
        {

        }
        /// <summary>
        /// Returns the alert output
        /// </summary>
        /// <param name="guildid"></param>
        /// <returns>detailed or short</returns>
        public static string GetAlertOutput(ulong guildid)
        {
            return "detailed";
        }
        /// <summary>
        /// Returns the data for a certain steamid
        /// </summary>
        /// <param name="guilid"></param>
        /// <returns></returns>
        public static SteamIDData GetSteamID(ulong guilid, ulong steamid)
        {
            return new SteamIDData() { SteamID = steamid };
        }
        /// <summary>
        /// Sets a custom name for a steamid withhin a guild(discord server)
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="steamid"></param>
        /// <param name="name"></param>
        public static void SetName(ulong guildid, SteamIDData steamid, string name)
        {

        }
        /// <summary>
        /// Assoziates a steamid with a tribe withhin a guild(discord server)
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="steamid"></param>
        /// <param name="tribe"></param>
        public static void SetTribe(ulong guildid, SteamIDData steamid, TribeData tribe)
        {

        }
    }
}