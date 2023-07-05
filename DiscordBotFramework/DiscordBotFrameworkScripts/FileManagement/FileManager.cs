using Discord.WebSocket;
using System.Globalization;

public static class FileManager
{
    public static Task AppendText(string _pathToFileAndFileExtension, string _content)
    {
        using (Stream stream = File.Open(_pathToFileAndFileExtension,
            FileMode.Append, FileAccess.Write, FileShare.Write))
        {
            using (StreamWriter sw = new StreamWriter(stream))
            {
                sw.WriteLine(_content);
                sw.Close();
            }
            stream.Close();
        }
        return Task.CompletedTask;
    }

    public async static void SaveFileAttachment(SocketMessage _message, string _filePath, string _fileName)
    {
        foreach (var attachment in _message.Attachments)
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(attachment.Url))
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        CheckIfFileAndPathExistsAndCreateItIfNecessary(_filePath, _fileName, stream);
                    }
                }
            }
        }
    }

    public async static void CheckIfFileAndPathExistsAndCreateItIfNecessary(
        string _filePath, string _fileName, System.IO.Stream? _optionalFileContent = null)
    {
        string _filePathWithFileName = _filePath + @"\" + _fileName;

        Log.WriteLine("Starting to create: " + _filePathWithFileName);
        if (!Directory.Exists(_filePath))
        {
            Directory.CreateDirectory(_filePath);
            Log.WriteLine("Done creating: " + _filePath);
        }
        else
        {
            Log.WriteLine("Already exists, did not create : " + _filePath);
        }

        Log.WriteLine("Starting to create: " + _filePathWithFileName);
        if (!File.Exists(_filePathWithFileName))
        {
            using (var fileStream = new FileStream(_filePathWithFileName, FileMode.CreateNew))
            {
                if (_optionalFileContent != null)
                {
                    Log.WriteLine("File has content, setting stream");
                    await _optionalFileContent.CopyToAsync(fileStream);
                }

                //await fileStream.CopyToAsync(fileStream);
                fileStream.Dispose();
                Log.WriteLine("Done creating: " + _filePathWithFileName);
            }
        }
        else if (File.Exists(_filePathWithFileName) && File.ReadAllText(_filePathWithFileName) == "0")
        {
            using (var fileStream = new FileStream(_filePathWithFileName, FileMode.Truncate))
            {
                if (_optionalFileContent != null)
                {
                    Log.WriteLine("File has content, setting stream");
                    await _optionalFileContent.CopyToAsync(fileStream);
                }

                //await fileStream.CopyToAsync(fileStream);
                fileStream.Dispose();
                Log.WriteLine("Already exists, truncated: " + _filePathWithFileName);
            }
        }
    }

    public async static void DeleteDirectoryIfItExists(string _directoryPath)
    {
        Log.WriteLine("Deleting: " + _directoryPath + " if it exists");
        if (!Directory.Exists(_directoryPath))
        {
            Log.WriteLine(_directoryPath + " did not exist, returning");
            return;
        }
        Log.WriteLine("Deleting: " + _directoryPath);
        Directory.Delete(_directoryPath, true);
        Log.WriteLine("Done deleting: " + _directoryPath);


        // Delete db here
        File.Delete(@"C:\DiscordBotFramework\Data\database.json");
        await SerializationManager.HandleDatabaseCreationOrLoading("0");
    }

    public async static void DeleteFileIfItExists(string _filePath)
    {
        Log.WriteLine("Deleting: " + _filePath + " if it exists");
        if (!File.Exists(_filePath))
        {
            Log.WriteLine(_filePath + " did not exist, returning");
            return;
        }
        Log.WriteLine("Deleting: " + _filePath);
        File.Delete(_filePath);
        Log.WriteLine("Done deleting: " + _filePath);
    }

    public static string GetFileTypeOfAnFile(string _fileName)
    {
        Log.WriteLine("Getting " + _fileName + "'s file type", LogLevel.DEBUG);

        string fileType = Path.GetExtension(_fileName);

        if (string.IsNullOrEmpty(fileType))
        {
            Log.WriteLine("FileType was null or empty!", LogLevel.CRITICAL);
            return "";
        }

        Log.WriteLine("Was not null, trimming " + fileType);

        fileType = fileType.TrimStart('.');

        Log.WriteLine("Returning: " + fileType, LogLevel.DEBUG);

        return fileType;
    }
}