using System;

namespace Caretaker
{
    public class PlayData
    {
        public long PlayTime = 0;
        public ServerData server;
        public bool Online = false;
        public long JoinedServer = 0;
        public int Joins = 0;
        public long CalculatePlayTime()
        {
            if (Online) return PlayTime + DateTime.Now.Ticks - JoinedServer;
            return PlayTime;
        }
    }
}
