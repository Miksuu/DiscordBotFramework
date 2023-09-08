using Discord;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.Serialization;

[DataContract]
public class DiscordBotDatabase : Database
{
    DiscordBotDatabase()
    {
        dataDirectory = DatabasePaths.discordDataDirectory;
        dbTempPathWithFileName = dataDirectory + @"\" + "database.tmp";
    }

    [DataMember] public Admins Admins = new Admins();
    [DataMember] public Categories Categories = new Categories();
    [DataMember] public EventScheduler EventScheduler = new EventScheduler();
}