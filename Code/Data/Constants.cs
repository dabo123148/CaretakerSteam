using System;
using System.Collections.Generic;
namespace Caretaker
{
    public static class Constants
    {
        //Time in min after which the server steamid update is executed
        public static readonly int UpdateSteamServerSteamIDDelay = 5;
        //Link for the repository where this code is stored, would be nice to leave it in here
        public static readonly string githuburl = "https://github.com/dabo123148/CaretakerSteam";
        public static readonly string supportserver = "https://discord.gg/NsXEKGthas";
        //UserInputLimits
        /// <summary>
        /// Max amount of characters a custom player name can have
        /// </summary>
        public static readonly int CustomPlayernameMaxLengthLimit = 100;
        /// <summary>
        /// Min amount of characters for custom player names, should never be below 2
        /// </summary>
        public static readonly int CustomPlayernameMinLengthLimit = 2;
        /// <summary>
        /// Amount of characters allowed for a mentioning by steamalert
        /// </summary>
        public static readonly int MentioningLengthLimit = 50;
        /// <summary>
        /// Says how many characters a tribename can minimally have, should never be below 2
        /// </summary>
        public static readonly int MinTribeNameLength = 2;
        /// <summary>
        /// Says how many characters a tribename can maximally have
        /// </summary>
        public static readonly int MaxTribeNameLength = 50;
        /// <summary>
        /// Depends mostly on your database, if your database can't handle it, then you should ignore them(probably better to use a permitted character list, but well whatever)
        /// </summary>
        public static readonly string InvalidTribenameCharacters = ";\\/:|!?%$§\"()[]=}{^°@€+~*#'<>-_";
        /// <summary>
        /// Limit of alerts a server can make
        /// </summary>
        public static readonly int ServerRelationshipLimit = 30;
        //SteamID scanning limits
        /// <summary>
        /// TIme in sec after which a server is seen as unresponsive for ServerScanning
        /// </summary>
        public static readonly long Serverunresponsiveafter = 300;
        /// <summary>
        /// Used to initilize an array in scan function, amount of different Relationships exist
        /// </summary>
        public static readonly int RelationshipCount = 6;
        /// <summary>
        /// Time in ms after which a steamid querry times out, only used by ServerScannerLinear.
        /// </summary>
        public static readonly int SteamIDQuerryTimeout = 1000;
        /// <summary>
        /// Time in ms after which a a2s querry times out.
        /// </summary>
        public static readonly int A2SQuerryTimeout = 1000;
        /// <summary>
        /// Limits the results of server searches, if too large might hit message character limit
        /// </summary>
        public static readonly int SearchResultLimit = 25;
        public static readonly string tribeunknown = "Bob/Not added tribe";
        public static readonly string servernotfoundmessage = "Server not found. This can happen if the server isn't part of the selected gamemode or if it isn't a PVP server.";
        public static readonly string unknowntribemessage = "Tribename unknown. Try creating it first using \"!createtribe\". Or check if it is maybe spelled differently using \"!listtribes\"";
        //This dashboard must be a message send by the bot, it will then take care of editing it with the scan statistics -> how many servers, how long it took etc
        public static readonly ulong DebugDashboardMessageID = 0;
        public static readonly ulong DebugDashboardMessageChannelID = 0;
    }
}