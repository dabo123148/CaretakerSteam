namespace Caretaker
{
    public class AlertData
    {
        public ulong GuildID;
        public ulong ChannelID;
        public Relationship Relationship;
        public string Mentions;
        public AlertData(ulong pGuildID, ulong pChannelID, Relationship pRelationship, string pMentions)
        {
            GuildID = pGuildID;
            ChannelID = pChannelID;
            Relationship = pRelationship;
            Mentions = pMentions;
        }
    }
}
