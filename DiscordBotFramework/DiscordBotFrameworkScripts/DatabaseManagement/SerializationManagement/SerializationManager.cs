using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;

public static class SerializationManager
{
    static string dbTempFileName = "database.tmp";
    static string dbTempPathWithFileName = DiscordBotDatabase.mainAppnameDataDir + @"\" + dbTempFileName;
    static SemaphoreSlim semaphore = new SemaphoreSlim(1);

    public static async Task SerializeDB()
    {
        await semaphore.WaitAsync();
        try
        {
            Log.WriteLine("SERIALIZING DB", LogLevel.SERIALIZATION);

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
                serializer.Serialize(writer, DiscordBotDatabase.Instance, typeof(DiscordBotDatabase));
                writer.Close();
                sw.Close();
            }

            FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(DiscordBotDatabase.mainAppnameDataDir, DiscordBotDatabase.dbFileName);
            File.Replace(dbTempPathWithFileName, DiscordBotDatabase.dbPathWithFileName, null);
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.CRITICAL);
            throw new InvalidOperationException(ex.Message);
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

    public static Task DeSerializeDB()
    {
        try
        {
            Log.WriteLine("DESERIALIZATION STARTING!", LogLevel.SERIALIZATION);

            FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(DiscordBotDatabase.mainAppnameDataDir, DiscordBotDatabase.dbFileName);

            string json = File.ReadAllText(DiscordBotDatabase.dbPathWithFileName);

            HandleDatabaseCreationOrLoading(json);

            Log.WriteLine("DB DESERIALIZATION DONE!", LogLevel.SERIALIZATION);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.CRITICAL);
            throw new InvalidOperationException(ex.Message);
        }
    }

    // _json param to 0 to force creation of the new db
    public static Task HandleDatabaseCreationOrLoading(string _json)
    {
        try
        {
            if (_json == "0")
            {
                //FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(dbPath, dbFileName);
                DiscordBotDatabase.Instance = new();
                Log.WriteLine("json was " + _json + ", creating a new db instance", LogLevel.DEBUG);

                return Task.CompletedTask;
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.NullValueHandling = NullValueHandling.Include;
            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;

            var newDeserializedObject = JsonConvert.DeserializeObject<DiscordBotDatabase>(_json, settings);

            if (newDeserializedObject == null)
            {
                return Task.CompletedTask;
            }

            DiscordBotDatabase.Instance = newDeserializedObject;

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.CRITICAL);
            throw new InvalidOperationException(ex.Message);
        }
    }
}