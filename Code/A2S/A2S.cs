using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
namespace Caretaker
{
    public static class A2S
    {
        public static byte[] receive(UdpClient client,IPEndPoint point)
        {
            byte[] response = client.Receive(ref point);
            return response;
        }
        public static string[] GetPlayerNames(string IP)
        {
            List<PlayerData> spieler = GetPlayers(IP);
            string[] rg = new string[spieler.Count];
            for(int a = 0; a < spieler.Count; a++)
            {
                rg[a] = spieler.ElementAt(a).name;
            }
            return rg;
        }
        public static string[] GetPlayersWithTime(string IP)
        {
            List<PlayerData> spieler = GetPlayers(IP);
            string[] rg = new string[spieler.Count];
            for (int a = 0; a < spieler.Count; a++)
            {
                TimeSpan duration = spieler.ElementAt(a).duration;
                rg[a] = duration.Days + ":";
                if (duration.Hours < 10) rg[a] += "0";
                rg[a] += duration.Hours + ":";
                if (duration.Minutes < 10) rg[a] += "0";
                rg[a] += duration.Minutes + ":";
                if (duration.Seconds < 10) rg[a] += "0";
                rg[a] += duration.Seconds + " - " + spieler.ElementAt(a).name;
            }
            return rg;
        }
        public static A2S_InfoResponse GetInfo(string IP)
        {
            A2S_InfoResponse rg = new A2S_InfoResponse();
            rg.IP = IP;
            try
            {
                if (IP.Split(':').Length != 2) throw new Exception("IP invalid, probably missing port");
                UdpClient client = new UdpClient();
                client.Client.SendTimeout = Constants.A2SQuerryTimeout;
                client.Client.ReceiveTimeout = Constants.A2SQuerryTimeout;
                IPEndPoint point = new IPEndPoint(IPAddress.Parse(IP.Split(':')[0]), int.Parse(IP.Split(':')[1]));
                client.Connect(point);
                byte[] req = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00 };
                client.Send(req, req.Length);
                byte[] response = receive(client, point);
                if (response.Length < 6)
                {
                    throw new ArgumentException("Server didn't return data");
                }
                if (!response[4].Equals(0x49)) throw new ArgumentException("Server returned invalid data");
                //Protocol = response[5]
                ByteIterator biterator = new ByteIterator(response, 6);
                //Name normally also contains version, we remove this here and just let name stand
                rg.Name = biterator.readstring().Split(' ')[0];
                rg.Map = biterator.readstring();
                //Folder
                biterator.readstring();
                rg.Game = biterator.readstring();
                rg.ID = biterator.readshort();
                rg.Players = biterator.next();
                rg.MaxPlayers = biterator.next();
                rg.bots = biterator.next();
                //Servertype + enviroment + visibility
                biterator.next();
                biterator.next();
                biterator.next();
                rg.VAC = biterator.next().Equals(0x01);
                //Not the ship -> ignoring the possible data
                rg.Version = biterator.readstring();
                if (biterator.hasNext())
                {
                    byte EDF = biterator.next();
                    rg.Port = biterator.readshort();
                    rg.Keywords = biterator.readstring();
                    rg.SteamID = ulong.Parse(rg.Keywords.Split(',')[1].Split(':')[1]);
                    if ((EDF & 0x01) == 1)
                    {
                        rg.GameID = biterator.readlong();
                    }
                }
            }catch(TimeoutException ex)
            {
                throw new Exception("Could not get info data. Maybe server is offline/temporary outtage");
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new Exception("Keywords string not readable -> can not extract steamid");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return rg;
        }
        public static List<PlayerData> GetPlayers(string IP)
        {
            List<PlayerData> rg = new List<PlayerData>();
            try
            {
                if (IP.Split(':').Length != 2) throw new Exception("IP invalid, probably missing port");
                UdpClient client = new UdpClient();
                client.Client.SendTimeout = Constants.A2SQuerryTimeout;
                client.Client.ReceiveTimeout = Constants.A2SQuerryTimeout;
                IPEndPoint point = new IPEndPoint(IPAddress.Parse(IP.Split(':')[0]), int.Parse(IP.Split(':')[1]));
                client.Connect(point);
                byte[] req = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x55, 0xFF, 0xFF, 0xFF, 0xFF };
                client.Send(req, req.Length);
                byte[] response = receive(client, point);
                //Initial connection(only needs to be executed once every min or so)
                if (response[4].Equals(0x41))
                {
                    for (int a = 5; a <= 8; a++) req[a] = response[a];
                    client.Send(req, req.Length);
                    response = receive(client, point);
                }
                //Sometimes server can be bugged and return some random data(really rare)
                int playercount = response[5];
                if (!response[4].Equals(0x44) || playercount == 255) throw new ArgumentException("Server returned invalid data");
                ByteIterator biterator = new ByteIterator(response, 6);
                //Reads out the player data
                for (int a = 0; a < playercount; a++)
                {
                    int pindex = biterator.next();
                    string pname = biterator.readstring();
                    long pscore = biterator.readlong();
                    TimeSpan pduration = TimeSpan.FromSeconds(biterator.readfloat());
                    if (pname.Length > 0) {
                        rg.Add(new PlayerData()
                        {
                            index = pindex,
                            name = pname,
                            score = pscore,
                            duration = pduration
                        });
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Could not get players. Maybe server is offline/temporary outtage");
            }
            return rg;
        }
    }
}
