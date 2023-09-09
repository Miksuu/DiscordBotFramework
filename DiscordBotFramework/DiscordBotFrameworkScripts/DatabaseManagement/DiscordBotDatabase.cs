using System.Runtime.Serialization;

[DataContract]
public class DiscordBotDatabase : Database
{
    public DiscordBotDatabase()
    {
        dataDirectory = DatabasePaths.discordDataDirectory;
        dbTempPathWithFileName = dataDirectory + @"\" + "database.tmp";
    }

    [DataMember] public Admins Admins = new Admins();
    [DataMember] public Categories Categories = new Categories();
    [DataMember] public EventScheduler EventScheduler = new EventScheduler();
}
