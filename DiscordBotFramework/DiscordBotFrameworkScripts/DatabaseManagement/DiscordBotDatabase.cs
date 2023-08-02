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

    static string appName = GetApplicationName();

    // File paths
    public static string mainAppnameDataDirectory = @"C:\" + appName + @"\Data\";
    public static string discordDataDir = mainAppnameDataDirectory + @"\DiscordBotDatabase";
    public static string dbPathWithFileName = discordDataDir + @"\" + "database.json";

    static string dbTempFileName = "database.tmp";
    public static string dbTempPathWithFileName = DiscordBotDatabase.dbPathWithFileName + @"\" + dbTempFileName;

    // The Database components
    [DataMember] public Admins Admins = new Admins();
    [DataMember] public Categories Categories = new Categories();
    [DataMember] public EventScheduler EventScheduler = new EventScheduler();

    static string GetApplicationName()
    {
        // Get the current assembly (the assembly where your application is defined)
        Assembly assembly = Assembly.GetEntryAssembly();

        // Get the assembly's full name, which includes the application name
        string assemblyName = assembly?.GetName()?.FullName;

        // Extract the application name from the full name
        // The application name is the part before the first comma in the full name
        int commaIndex = assemblyName.IndexOf(',');
        string appName = (commaIndex > 0) ? assemblyName.Substring(0, commaIndex) : assemblyName;

        return appName;
    }
}