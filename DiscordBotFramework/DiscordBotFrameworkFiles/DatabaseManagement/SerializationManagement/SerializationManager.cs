﻿using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;

public static class SerializationManager
{
    static string dbTempFileName = "database.tmp";
    static string dbTempPathWithFileName = Database.dbPath + @"\" + dbTempFileName;
    static SemaphoreSlim semaphore = new SemaphoreSlim(1);

    public static async Task SerializeDB(bool _circularDependency = false)
    {
        await semaphore.WaitAsync();
        try
        {
            Log.WriteLine("SERIALIZING DB", LogLevel.SERIALIZATION);

            if (!_circularDependency)
            {
                await SerializeUsersOnTheServer();
            }

            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
            serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
            serializer.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
            serializer.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All;
            serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
            serializer.ObjectCreationHandling = ObjectCreationHandling.Replace;
            serializer.ContractResolver = new DataMemberContractResolver();

            using (StreamWriter sw = new StreamWriter(dbTempPathWithFileName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, Database.Instance, typeof(Database));
                writer.Close();
                sw.Close();
            }

            FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(Database.dbPath, Database.dbFileName);
            File.Replace(dbTempPathWithFileName, Database.dbPathWithFileName, null);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public class DataMemberContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            return properties.Where(p => p.AttributeProvider.GetAttributes(typeof(DataMemberAttribute), true).Any()).ToList();
        }
    }

    public static Task SerializeUsersOnTheServer()
    {
        Log.WriteLine("Serializing users on the server", LogLevel.SERIALIZATION);

        var guild = BotReference.GetGuildRef();

        foreach (SocketGuildUser user in guild.Users)
        {
            if (user == null)
            {
                Log.WriteLine("User was null!", LogLevel.ERROR);
            }
            else
            {
                // Move to method
                string userString = user.Username + " (" + user.Id + ")";
                Log.WriteLine("Looping on: " + userString);

                if (!user.IsBot)
                {
                    Database.Instance.CachedUsers.AddUserIdToCachedConcurrentBag(user.Id);
                }
                else
                {
                    Log.WriteLine(userString + " is a bot, disregarding.");
                }
            }
        }
        Log.WriteLine("Done looping through current users.");

        return Task.CompletedTask;
    }

    public static Task DeSerializeDB()
    {
        Log.WriteLine("DESERIALIZATION STARTING!", LogLevel.SERIALIZATION);

        FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(Database.dbPath, Database.dbFileName);

        string json = File.ReadAllText(Database.dbPathWithFileName);

        HandleDatabaseCreationOrLoading(json);

        Log.WriteLine("DB DESERIALIZATION DONE!", LogLevel.SERIALIZATION);

        return Task.CompletedTask;
    }

    // _json param to 0 to force creation of the new db
    public static Task HandleDatabaseCreationOrLoading(string _json)
    {
        if (_json == "0")
        {
            //FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(dbPath, dbFileName);
            Database.Instance = new();
            Log.WriteLine("json was " + _json + ", creating a new db instance", LogLevel.DEBUG);

            return Task.CompletedTask;
        }

        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.Auto;
        settings.NullValueHandling = NullValueHandling.Include;
        settings.ObjectCreationHandling = ObjectCreationHandling.Replace;

        var newDeserializedObject = JsonConvert.DeserializeObject<Database>(_json, settings);

        if (newDeserializedObject == null)
        {
            return Task.CompletedTask;
        }

        Database.Instance = newDeserializedObject;

        return Task.CompletedTask;
    }
}