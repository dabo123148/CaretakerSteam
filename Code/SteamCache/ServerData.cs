using Steamworks;
using System;
using System.Collections.Generic;

namespace Caretaker
{
    public class ServerData
    {
        public string Name;
        public string IP;
        public List<AlertData> alerts = new List<AlertData>();
        public List<ulong> ConnectedIDs = new List<ulong>();
        public bool Crashed = false;
        public CSteamID ServerSteamID;
        public long LastUpdateTime;
        public bool Connected = false;
        public int MaxPlayers = 70;
        /// <summary>
        /// Init
        /// </summary>
        /// <param name="pName">name</param>
        /// <param name="pIP">ip</param>
        /// <param name="pMaxPlayers">max player count on server</param>
        /// <param name="ForScan">Loads server steamid, connected players and alerts</param>
        public ServerData(string pName, string pIP,int pMaxPlayers, bool ForScan = false)
        {
            LastUpdateTime = DateTime.Now.Ticks-(Constants.Serverunresponsiveafter*TimeSpan.TicksPerSecond-TimeSpan.TicksPerMinute);
            Name = pName;
            IP = pIP;
            MaxPlayers = pMaxPlayers;
            if (ForScan)
            {
                ServerSteamID = new CSteamID(DataBase.GetServerSteamID(this));
                //Here was a loading of crashed status from database, but probably not needed anymore, since there is no longer a message that server is unresponsive
                ConnectedIDs = DataBase.LoadConnectedPlayers(this);
                alerts = DataBase.GetAlerts(this);
                //Here was a loading of the lastupdatedtime, but other then for statistics that isn't needed, so for now it is removed aswell
            }
        }
    }
}