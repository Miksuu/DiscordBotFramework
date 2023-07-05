using Discord.WebSocket;

public static class CommandHandler
{
    // Installs the commands that are predefined in the code itself
    public async static Task InstallCommandsAsync()
    {
        try
        {
            Log.WriteLine("Starting to install the commands.", LogLevel.DEBUG);

            await PrepareCommands();

            var client = BotReference.GetClientRef();
            client.SlashCommandExecuted += SlashCommandHandler;

            Log.WriteLine("Done installing the commands.", LogLevel.DEBUG);
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.CRITICAL);
            throw new InvalidOperationException(ex.Message);
        }
    }

    private static async Task SlashCommandHandler(SocketSlashCommand _command)
    {
        try
        {
            Log.WriteLine("Start of SlashCommandHandler. OptionsCount: " + _command.Data.Options.Count +
                " by user: " + _command.User.Id, LogLevel.DEBUG);

            string? firstOptionString = string.Empty;

            var firstOption = _command.Data.Options.FirstOrDefault();
            if (firstOption == null)
            {
                Log.WriteLine(nameof(firstOption) + " was null! ", LogLevel.ERROR);
                return;
            }

            firstOptionString = firstOption.Value.ToString();
            if (firstOptionString == null)
            {
                Log.WriteLine("firstOptionString was null! ", LogLevel.ERROR);
                return;
            }

            Log.WriteLine("The command " + _command.Data.Name + " had " + _command.Data.Options.Count + " options in it." +
                " The first command had an argument: " + firstOptionString, LogLevel.DEBUG);

            InterfaceCommand interfaceCommand = GetCommandInstance(_command.CommandName.ToUpper().ToString());
            var response = await interfaceCommand.ReceiveCommandAndCheckForAdminRights(_command, firstOptionString);
            if (response.serialize)
            {
                await SerializationManager.SerializeDB();
            }

            await _command.RespondAsync(BotMessaging.GetMessageResponse(
                _command.Data.Name, response.responseString, _command.Channel.Name), ephemeral: true);

            Log.WriteLine("Handling slashcommand: " + firstOptionString + " by user: " + _command.User.Id + " done", LogLevel.DEBUG);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("error 50006"))
            {
                Log.WriteLine("skipped empty message try-catch");
                return;
            }

            Log.WriteLine(ex.Message, LogLevel.CRITICAL);
            return;
        }
    }

    private static async Task PrepareCommands()
    {
        try
        {
            var commandEnumValues = Enum.GetValues(typeof(CommandName));
            Log.WriteLine("Starting to prepare the commands with count: " + commandEnumValues.Length, LogLevel.DEBUG);

            foreach (CommandName commandName in commandEnumValues)
            {
                Log.WriteLine("Looping on cmd" + nameof(commandName));

                try
                {
                    InterfaceCommand interfaceCommand = GetCommandInstance(commandName.ToString());
                    await interfaceCommand.AddNewCommandWithOption();
                }
                catch (Exception ex)
                {
                    Log.WriteLine(ex.Message, LogLevel.CRITICAL);
                    continue;
                }
            }
            Log.WriteLine("Done preparing the commands.");
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.CRITICAL);
            throw new InvalidOperationException(ex.Message);
        }
    }

    private static InterfaceCommand GetCommandInstance(string _commandName)
    {
        try
        {
            return (InterfaceCommand)EnumExtensions.GetInstance(_commandName.ToString());
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.CRITICAL);
            throw new InvalidOperationException(ex.Message);
        }
    }
}