using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
namespace Caretaker
{
    public class Compfort
    {
        private Program main;
        private MessageClass current;
        private MessageClass last;
        public Compfort(Program pmain)
        {
            main = pmain;
        }
        public static bool IsSteamID(string steamid)
        {
            if (steamid.Length != 17) return false;
            if (!ulong.TryParse(steamid, out ulong value)) return false;
            if (!steamid.StartsWith("7656")) return false;
            return true;
        }
        public static string MultipleServersFound(List<ServerData> sData, ulong authorid, ulong channelid, string before, string after)
        {
            string rg = "Multiple servers have been found with that name. Please be more specific. You can also use !setgamemode to limit the results in the future:";
            List<String> choices = new List<string>();
            for (int a = 0; a < sData.Count && a < Constants.SearchResultLimit; a++)
            {
                string servername = sData.ElementAt(a).Name;
                rg += "```fix\n(" + (a + 1) + ") " + servername + "```";
                choices.Add(before + servername + after);
            }
            if (sData.Count >= Constants.SearchResultLimit + 1)
            {
                rg += "```fix\nResult limit hit```";
            }
            DataBase.OfferChoice(channelid, authorid, choices);
            return rg;
        }
        public static string[] ExtractAllvalues(string page, string parametername)
        {
            string[] rg = new string[0];
            for (int A = 0; A < page.Length - parametername.Length; A++)
            {
                for (int B = 0; B < parametername.Length; B++)
                {
                    if (B == parametername.Length - 1)
                    {
                        string value = "";
                        for (int C = A + B + 1; C < page.Length; C++)
                        {
                            if (page[C] != '"' && page[C] != ',')
                            {
                                value += page[C];
                            }
                            else
                            {
                                C = page.Length;
                                Array.Resize(ref rg, rg.Length + 1);
                                rg[rg.Length - 1] = value;
                            }
                        }
                    }
                    else
                    {
                        if (parametername[B].CompareTo(page[A + B]) != 0)
                        {
                            B = parametername.Length;
                        }
                    }
                }
            }
            return rg;
        }
        public void AddMessage(ulong channelid, string message)
        {
            if (current == null)
            {
                current = new MessageClass() { ID = 0 };
                last = current;
            }
            //Compfort.Log("added " + message + " after " + last.Message);
            last.next = new MessageClass() { channelid = channelid, Message = message, ID = last.ID + 1 };
            last = last.next;
        }
        public async Task postMessageTask()
        {
            Compfort.Log("Started PostMessageTask");
            await Task.Delay(1);
            try
            {
                while (true)
                {
                    if (current != null)
                    {
                        if (current.next != null)
                        {
                            current = current.next;
                            await postMessage(current.channelid, current.Message);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Compfort.Log("PostMessageTask crashed", LogType.Crash);
            }
        }
        public async Task postMessage(ulong channelid, string message)
        {
            if (message.Length > 2000)
            {
                Log("Message lenght limit hit " + message.Length + "\n" + message, LogType.Crash);
                return;
            }
            try
            {
                IMessageChannel chnl = main._client.GetChannel(channelid) as IMessageChannel;
                //Guildname = ((SocketGuildChannel)chnl).Guild.Name;
                if (chnl != null)
                {
                    SocketGuildChannel gchnl = (SocketGuildChannel)chnl;
                    await chnl.SendMessageAsync(message);
                }
                else
                {
                    Compfort.Log("Channel not found: " + channelid);
                }
            }
            catch (Discord.Net.HttpException ex)
            {
                if (ex.DiscordCode == null || (ex.DiscordCode != 50013 && ex.DiscordCode != 50001))
                {
                    Compfort.Log("Exception for channel: " + channelid + " - " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public async Task postMessages(ulong channelid, List<String> messages)
        {
            try
            {
                IMessageChannel chnl = main._client.GetChannel(channelid) as IMessageChannel;
                if (chnl != null)
                {
                    SocketGuildChannel gchnl = (SocketGuildChannel)chnl;
                    foreach (String message in messages)
                    {
                        await chnl.SendMessageAsync(message);
                    }
                }
                else
                {
                    Compfort.Log("Channel not found: " + channelid);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public static bool ContainsString(String[] arr, String match)
        {
            foreach (string s in arr)
            {
                if (s.CompareTo(match) == 0) return true;
            }
            return false;
        }
        public static string RemoveChars(string s, string c)
        {
            string rg = "";
            foreach (char character in s)
            {
                bool found = false;
                foreach (char chara in c)
                {
                    if (chara == character)
                    {
                        found = true;
                    }
                }
                if (found == false)
                {
                    rg += character;
                }
            }
            return rg;
        }
        /// <summary>
        /// Logs into the console, Logtype is in this version due to simplicity reasons ignored
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="type"></param>
        public static void Log(string entry, LogType type = LogType.Other)
        {
            entry = DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + ":" + entry;
            Console.WriteLine(entry);
        }
        public static TribeData GetTribeByID(ulong guildid, int ID)
        {
            List<TribeData> knownTribes = DataBase.GetTribes(guildid);
            foreach (TribeData tribe in knownTribes)
            {
                if (tribe.ID == ID) return tribe;
            }
            throw new Exception("A tribe with the ID " + ID + " does not exist");
        }
        public static bool TribeExists(List<TribeData> knowntribes, string newname)
        {
            foreach (TribeData tribe in knowntribes)
            {
                if (tribe.name.ToLower().CompareTo(newname.ToLower()) == 0) return true;
            }
            return false;
        }
        public static Relationship ExtractRelationship(string s)
        {
            s = s.ToLower();
            Relationship relationship = Relationship.invalid;
            if (s == "unknown") relationship = Relationship.unknown;
            if (s == "enemy") relationship = Relationship.enemy;
            if (s == "neutral") relationship = Relationship.neutral;
            if (s == "allied" || s == "ally") relationship = Relationship.allied;
            if (s == "friendly") relationship = Relationship.friendly;
            if (s == "tribe" || s == "tribemember") relationship = Relationship.tribemember;
            if (s == "beta") relationship = Relationship.beta;
            return relationship;
        }
        public static Relationship IntToRelationship(int i)
        {
            switch (i)
            {
                case 0:
                    return Relationship.unknown;
                case 1:
                    return Relationship.enemy;
                case 2:
                    return Relationship.neutral;
                case 3:
                    return Relationship.allied;
                case 4:
                    return Relationship.friendly;
                case 5:
                    return Relationship.tribemember;
                case 6:
                    return Relationship.beta;
            }
            return Relationship.unknown;
        }
        public static string RelationshipToString(int relationship)
        {
            return RelationshipToString(IntToRelationship(relationship));
        }
        public static string RelationshipToString(Relationship relationship)
        {
            if (relationship == Relationship.unknown) return "Unknown";
            if (relationship == Relationship.enemy) return "Enemy";
            if (relationship == Relationship.neutral) return "Neutral";
            if (relationship == Relationship.allied) return "Allied";
            if (relationship == Relationship.friendly) return "Friendly";
            if (relationship == Relationship.tribemember) return "Tribemember";
            if (relationship == Relationship.beta) return "Beta";
            return "Error unknown Relationship";
        }
        public static string CreateTribeString(ulong GuildID, ServerData sData)
        {
            string rg = "";
            List<TribeData> tribesonserver = DataBase.GetTribes(GuildID, sData);
            foreach (TribeData tribe in tribesonserver)
            {
                if (rg != "") rg += ", ";
                rg += tribe.name;
            }
            return rg;
        }
        public static string ToTimeString(long Ticks)
        {
            string days = Math.Floor((double)Ticks / (double)TimeSpan.TicksPerDay).ToString();
            string houers = Math.Floor(((double)Ticks % (double)TimeSpan.TicksPerDay) / (double)TimeSpan.TicksPerHour).ToString();
            if (houers.Length == 1)
            {
                houers = "0" + houers;
            }
            string mins = Math.Floor((((double)Ticks % (double)TimeSpan.TicksPerDay) % (double)TimeSpan.TicksPerHour) / TimeSpan.TicksPerMinute).ToString();
            if (mins.Length == 1)
            {
                mins = "0" + mins;
            }
            string secounds = Math.Floor(((((double)Ticks % (double)TimeSpan.TicksPerDay) % (double)TimeSpan.TicksPerHour) % TimeSpan.TicksPerMinute) / TimeSpan.TicksPerSecond).ToString();
            if (secounds.Length == 1)
            {
                secounds = "0" + secounds;
            }
            return days + ":" + houers + ":" + mins + ":" + secounds;
        }
        private class MessageClass
        {
            public ulong channelid;
            public MessageClass next;
            public string Message;
            public int ID;
        }
    }
}
