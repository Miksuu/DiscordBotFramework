using Discord;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.Serialization;

[DataContract]
public class Database : Singleton<Database>
{
    public string dataDirectory = string.Empty;
    public string dbTempPathWithFileName = string.Empty;

    public void SerializeDatabase(Newtonsoft.Json.JsonSerializer _serializer)
    {
        Log.WriteLine("SERIALIZATION STARTING FOR " + dataDirectory, LogLevel.SERIALIZATION);

        if (!Directory.Exists(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }

        using (StreamWriter sw = new StreamWriter(dbTempPathWithFileName))
        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            _serializer.Serialize(writer, this, typeof(Database)); // Might need to change this to the derived class
            writer.Close();
            sw.Close();
        }

        FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(dataDirectory, @"\database.tmp");
        FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(dataDirectory, @"\database.json");
        File.Replace(dbTempPathWithFileName, dataDirectory + @"\database.json", null);

        Log.WriteLine("DONE SERIALIZATION FOR " + dataDirectory, LogLevel.SERIALIZATION);
    }

    public Task DeserializeDatabase(Type _type)
    {
        try
        {
            Log.WriteLine("DESERIALIZATION STARTING FOR " + dataDirectory, LogLevel.SERIALIZATION);

            FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(dataDirectory, "database.json");

            string json = File.ReadAllText(dataDirectory + @"\database.json");

            HandleDatabaseCreationOrLoading(json, _type);

            Log.WriteLine("DONE DESERIALIZATION FOR " + dataDirectory, LogLevel.SERIALIZATION);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }

    public static Task HandleDatabaseCreationOrLoading(string _json, Type _type)
    {
        try
        {
            if (_json == "0")
            {
                //FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(dbPath, dbFileName);
                Instance = new();
                Log.WriteLine("json was " + _json + ", creating a new db instance", LogLevel.DEBUG);
                return Task.CompletedTask;
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.NullValueHandling = NullValueHandling.Include;
            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;

            var newDeserializedObject = JsonConvert.DeserializeObject(_json, _type, settings);

            if (newDeserializedObject == null)
            {
                return Task.CompletedTask;
            }

            Instance = (DiscordBotDatabase)newDeserializedObject;

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }

}