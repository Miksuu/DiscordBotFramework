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

    // File paths for the discord database
    public static string mainAppnameDirectory = @"C:\" + appName;
    public static string mainAppnameDataDirectory = mainAppnameDirectory + @"\Data";
    public static string discordDataDirectory = mainAppnameDataDirectory + @"\DiscordBotDatabase";

    static string discordDbTempFileName = "database.tmp";
    public static string discorDdbTempPathWithFileName = discordDataDirectory + @"\" + discordDbTempFileName;

    // File paths for the database
    public static string applicationDataDirectory = mainAppnameDataDirectory + @"\Database";

    static string dbTempFileName = "database.tmp";
    public static string dbTempPathWithFileName = applicationDataDirectory + @"\" + dbTempFileName;

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