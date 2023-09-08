﻿using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Runtime.Serialization;

public static class SerializationManager
{

    static SemaphoreSlim semaphore = new SemaphoreSlim(1);

    static List<Database> listOfDatabaseInstances = new List<Database>();
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

            if (!Directory.Exists(DatabasePaths.mainAppnameDataDirectory))
            {
                Directory.CreateDirectory(DatabasePaths.mainAppnameDataDirectory);
            }

            foreach (var dbInstance in listOfDatabaseInstances)
            {
                try
                {
                    dbInstance.SerializeDatabase(serializer);

                    //if (dbStringLocationKvp.Key == 0)
                    //{

                    //}
                    //else
                    //{
                    //    using (StreamWriter sw = new StreamWriter(DatabasePaths.dbTempPathWithFileName))
                    //    using (JsonWriter writer = new JsonTextWriter(sw))
                    //    {
                    //        serializer.Serialize(writer, ApplicationDatabase.Instance, typeof(ApplicationDatabase));
                    //        writer.Close();
                    //        sw.Close();
                    //    }

                    //    FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(DatabasePaths.applicationDataDirectory, @"\database.tmp");
                    //    FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(DatabasePaths.applicationDataDirectory, @"\database.json");
                    //    File.Replace(DatabasePaths.dbTempPathWithFileName, DatabasePaths.applicationDataDirectory + @"\database.json", null);
                    //}
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

    public static Task DeSerializeDB()
    {
        try
        {
            Log.WriteLine("DESERIALIZATION STARTING!", LogLevel.SERIALIZATION);

            foreach (var dbStringLocationKvp in listOfDbNames)
            {
                FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(dbStringLocationKvp.Value, "database.json");

                string json = File.ReadAllText(dbStringLocationKvp.Value + @"\database.json");

                if (dbStringLocationKvp.Key == 0)
                {
                    HandleDiscordBotDatabaseCreationOrLoading(json);
                } 
                else
                {
                    HandleDatabaseCreationOrLoading(json);
                }
            }

            Log.WriteLine("DB DESERIALIZATION DONE!", LogLevel.SERIALIZATION);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }

    // _json param to 0 to force creation of the new db
    public static Task HandleDiscordBotDatabaseCreationOrLoading(string _json)
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
            Log.WriteLine(ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }

    public static Task HandleDatabaseCreationOrLoading(string _json)
    {
        try
        {
            if (_json == "0")
            {
                //FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(dbPath, dbFileName);
                ApplicationDatabase.Instance = new();
                Log.WriteLine("json was " + _json + ", creating a new db instance", LogLevel.DEBUG);

                return Task.CompletedTask;
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.NullValueHandling = NullValueHandling.Include;
            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;

            var newDeserializedObject = JsonConvert.DeserializeObject<ApplicationDatabase>(_json, settings);

            if (newDeserializedObject == null)
            {
                return Task.CompletedTask;
            }

            ApplicationDatabase.Instance = newDeserializedObject;

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }
}