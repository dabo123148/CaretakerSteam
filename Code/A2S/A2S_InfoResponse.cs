using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caretaker
{
    public class A2S_InfoResponse
    {
        public string IP;
        //Protocol
        public string Name;
        public string Map;
        public string Game;
        public uint ID;
        public int Players;
        public int MaxPlayers;
        public int bots;
        //Server type
        //Environment
        public bool Public;
        public bool VAC;
        //The ship(not relevant for ark)
        public string Version;
        //Extra data
        public uint Port;
        public ulong SteamID;
        public long GameID;
        public string Keywords;
    }
}
