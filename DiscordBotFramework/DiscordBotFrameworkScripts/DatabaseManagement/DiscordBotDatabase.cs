using Discord;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.Serialization;

[DataContract]
public class DiscordBotDatabase : Singleton<DiscordBotDatabase>
{
    // The Database components
    [DataMember] public Admins Admins = new Admins();
    [DataMember] public Categories Categories = new Categories();
    [DataMember] public EventScheduler EventScheduler = new EventScheduler();
}