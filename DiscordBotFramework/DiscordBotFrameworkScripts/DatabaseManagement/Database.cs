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
        if (!Directory.Exists(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }

        using (StreamWriter sw = new StreamWriter(DatabasePaths.discordDbTempPathWithFileName))
        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            _serializer.Serialize(writer, this, typeof(Database)); // Might need to change this to the derived class
            writer.Close();
            sw.Close();
        }

        FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(dataDirectory, @"\database.tmp");
        FileManager.CheckIfFileAndPathExistsAndCreateItIfNecessary(dataDirectory, @"\database.json");
        File.Replace(dbTempPathWithFileName, dataDirectory + @"\database.json", null);
    }
}