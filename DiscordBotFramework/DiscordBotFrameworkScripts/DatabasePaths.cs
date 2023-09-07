using System.Reflection;

public static class DatabasePaths
{
    static string appName = GetApplicationName();

    // File paths for the discord database
    public static string mainAppnameDirectory = @"C:\" + appName;
    public static string mainAppnameDataDirectory = mainAppnameDirectory + @"\Data";
    public static string discordDataDirectory = mainAppnameDataDirectory + @"\DiscordBotDatabase";
    static string dbTempFileName = "database.tmp";
    public static string discordDbTempPathWithFileName = discordDataDirectory + @"\" + dbTempFileName;

    // File paths for the database
    public static string applicationDataDirectory = mainAppnameDataDirectory + @"\Database";
    public static string dbTempPathWithFileName = applicationDataDirectory + @"\" + dbTempFileName;

    private static string GetApplicationName()
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