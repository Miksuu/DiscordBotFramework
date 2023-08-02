using Discord;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.Serialization;

[DataContract]
public class DiscordBotDatabase
{
    public static DiscordBotDatabase Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new DiscordBotDatabase();
                }
                return instance;
            }
        }
        set
        {
            instance = value;
        }
    }

    // Singleton stuff
    private static DiscordBotDatabase? instance;
    private static readonly object padlock = new object();

    static string appName = Assembly.GetEntryAssembly()?.GetName()?.FullName;

    // File paths
    public static string mainAppnameDataDirectory = @"C:\" + appName + @"\Data\";
    public static string discordDataDir = mainAppnameDataDirectory + @"\DiscordBotDatabase";
    public static string dbFileName = "database.json";
    public static string dbPathWithFileName = discordDataDir + @"\" + dbFileName;

    // The Database components
    [DataMember] public Admins Admins = new Admins();
    [DataMember] public Categories Categories = new Categories();
    [DataMember] public EventScheduler EventScheduler = new EventScheduler();
}