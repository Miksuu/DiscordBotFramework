using Discord;
using System.Collections.Concurrent;
using System.Runtime.Serialization;

[DataContract]
public class Database
{
    public static Database Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new Database();
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
    private static Database? instance;
    private static readonly object padlock = new object();

    // File paths
    public static string dbPath = @"C:\DiscordBotFramework\Data";
    public static string dbFileName = "database.json";
    public static string dbPathWithFileName = dbPath + @"\" + dbFileName;

    // The Database components
    [DataMember] public Admins Admins = new Admins();
    [DataMember] public Categories Categories = new Categories();
    [DataMember] public EventScheduler EventScheduler = new EventScheduler();
}