using Discord;
using Discord.WebSocket;
using System.Data;

// Reference to the bot's client/guild reference variable
public class BotReference
{
    private static BotReference instance;
    private static readonly object lockObject = new object();

    public bool ConnectionState
    {
        get => connectionState.GetValue();
        set => connectionState.SetValue(value);
    }

    private static DiscordSocketClient clientRef;
    private static SocketGuild guildRef;
    private logVar<bool> connectionState = new logVar<bool>();

    private BotReference() { }

    public static BotReference Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new BotReference();
                    }
                }
            }
            return instance;
        }
    }

    public static DiscordSocketClient GetClientRef()
    {
        if (clientRef == null)
        {
            string errorMessage = "ClientRef was null!";
            Log.WriteLine(errorMessage, LogLevel.CRITICAL);
            throw new InvalidOperationException(errorMessage);
        }
        return clientRef;
    }

    public static DiscordSocketClient SetClientRefAndReturnIt()
    {
        var config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.All
        };

        clientRef = new DiscordSocketClient(config);
        return clientRef;
    }

    public static SocketGuild GetGuildRef()
    {
        if (clientRef == null)
        {
            string errorMessage = "Guild ref was null! wrong GuildID?";
            Log.WriteLine(errorMessage, LogLevel.CRITICAL);
            throw new InvalidOperationException(errorMessage);
        }

        if (guildRef == null)
        {
            guildRef = clientRef.GetGuild(Preferences.GuildID);
        }

        return guildRef;
    }

    public static ulong GetGuildID()
    {
        Log.WriteLine("Getting the " + nameof(Preferences.GuildID) + ": " +
            Preferences.GuildID);
        return Preferences.GuildID;
    }
}