using Newtonsoft.Json;
using System.Runtime.Serialization;

[DataContract]
public abstract class Database
{
    private static readonly Dictionary<Type, Database> instances = new Dictionary<Type, Database>();

    public static T GetInstance<T>() where T : Database, new()
    {
        if (!instances.ContainsKey(typeof(T)))
        {
            instances[typeof(T)] = new T();
        }
        return (T)instances[typeof(T)];
    }

    public static void SetInstance(Database instance)
    {
        Type type = instance.GetType();
        if (instances.ContainsKey(type))
        {
            instances[type] = instance;
        }
        else
        {
            instances.Add(type, instance);
        }
    }


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

            var discordDatabase = Database.GetInstance<DiscordBotDatabase>();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }

    private static Task HandleDatabaseCreationOrLoading(string _json, Type _type)
    {
        try
        {
            if (_json == "0")
            {
                var instance = Activator.CreateInstance(_type);
                SetInstance((Database)instance);
                Log.WriteLine("json was " + _json + ", creating a new db instance", LogLevel.DEBUG);
                return Task.CompletedTask;
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.NullValueHandling = NullValueHandling.Include;
            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;

            var newDeserializedObject = (Database)JsonConvert.DeserializeObject(_json, settings);

            if (newDeserializedObject == null)
            {
                return Task.CompletedTask;
            }

            SetInstance((Database)newDeserializedObject);

            //var discordDatabase = Database.GetInstance<DiscordBotDatabase>();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }
}