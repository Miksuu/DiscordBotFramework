using Newtonsoft.Json;

public sealed class Preferences
{
    public static Preferences Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    string json = File.ReadAllText("preferences.json");
                    instance = JsonConvert.DeserializeObject<Preferences>(json);
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
    private static Preferences? instance;
    private static readonly object padlock = new object();

    public ulong GuildID;
}
