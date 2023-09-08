using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Runtime.Serialization;

public static class SerializationManager
{
    static SemaphoreSlim semaphore = new SemaphoreSlim(1);

    public static List<Database> listOfDatabaseInstances =
        new List<Database>() { Database.GetInstance<DiscordBotDatabase>(), Database.GetInstance<ApplicationDatabase>() };
    public static async Task SerializeDB()
    {
        await semaphore.WaitAsync();
        try
        {
            Log.WriteLine("SERIALIZING DBs", LogLevel.SERIALIZATION);

            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
            serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
            serializer.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
            serializer.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All;
            serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
            serializer.ObjectCreationHandling = ObjectCreationHandling.Replace;
            serializer.ContractResolver = new DataMemberContractResolver();

            if (!Directory.Exists(DatabasePaths.mainAppnameDataDirectory))
            {
                Directory.CreateDirectory(DatabasePaths.mainAppnameDataDirectory);
            }

            foreach (var dbInstance in listOfDatabaseInstances)
            {
                try
                {
                    dbInstance.SerializeDatabase(serializer, dbInstance.GetType());
                }
                catch (Exception ex)
                {
                    Log.WriteLine(ex.Message, LogLevel.ERROR);
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.ERROR);
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

    public static void DeSerializeDatabases()
    {
        try
        {
            Log.WriteLine("SERIALIZING DB", LogLevel.SERIALIZATION);

            foreach (var dbInstance in listOfDatabaseInstances)
            {
                try
                {
                    var type = dbInstance.GetType();
                    dbInstance.DeSerializeDatabase(type);
                }
                catch (Exception ex)
                {
                    Log.WriteLine(ex.Message, LogLevel.ERROR);
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }
}