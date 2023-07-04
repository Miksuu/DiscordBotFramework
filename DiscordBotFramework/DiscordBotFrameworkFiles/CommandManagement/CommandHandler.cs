﻿using Discord.WebSocket;

public static class CommandHandler
{
    // Installs the commands that are predefined in the code itself
    public async static Task InstallCommandsAsync()
    {
        Log.WriteLine("Starting to install the commands.");

        var client = BotReference.GetClientRef();
        await PrepareCommands();

        client.SlashCommandExecuted += SlashCommandHandler;

        return;
    }

    private static async Task SlashCommandHandler(SocketSlashCommand _command)
    {
        try
        {
            Log.WriteLine("Start of SlashCommandHandler");

            string? firstOptionString = string.Empty;

            Log.WriteLine("OptionsCount: " + _command.Data.Options.Count);

            if (_command.Data.Options.Count > 0)
            {
                var firstOption = _command.Data.Options.FirstOrDefault();
                if (firstOption == null)
                {
                    Log.WriteLine(nameof(firstOption) + " was null! ", LogLevel.ERROR);
                    return;
                }

                firstOptionString = firstOption.Value.ToString();

                Log.WriteLine("The command " + _command.Data.Name + " had " + _command.Data.Options.Count + " options in it." +
                    " The first command had an argument: " + firstOptionString, LogLevel.DEBUG);

                // Add a for loop here to print the command arguments, if multiple later on.
            }
            else
            {
                Log.WriteLine("The command " + _command.Data.Name + " does not have any options in it.", LogLevel.DEBUG);
            }

            if (firstOptionString == null)
            {
                Log.WriteLine("firstOptionString was null! ", LogLevel.ERROR);
                return;
            }

            InterfaceCommand interfaceCommand = GetCommandInstance(_command.CommandName.ToUpper().ToString());

            var responseTuple = await interfaceCommand.ReceiveCommandAndCheckForAdminRights(_command, firstOptionString);

            if (responseTuple.serialize)
            {
                await SerializationManager.SerializeDB();
            }

            await _command.RespondAsync(BotMessaging.GetMessageResponse(
                _command.Data.Name, responseTuple.responseString, _command.Channel.Name), ephemeral: true);

            Log.WriteLine("Sending and responding to the message done.");
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

    public static async Task PrepareCommands()
    {
        Log.WriteLine("Starting to prepare the commands.");

        var commandEnumValues = Enum.GetValues(typeof(CommandName));
        Log.WriteLine(nameof(commandEnumValues) +
            " length: " + commandEnumValues.Length);

        var client = BotReference.GetClientRef();

        foreach (CommandName commandName in commandEnumValues)
        {
            Log.WriteLine("Looping on cmd" + nameof(commandName));

            try
            {
                InterfaceCommand interfaceCommand = GetCommandInstance(commandName.ToString());

                // For commands without option, need to implement it with null check
                await interfaceCommand.AddNewCommandWithOption(client);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message, LogLevel.CRITICAL);
                continue;
            }
        }

        /*
        // Command for showing a test gif
        AddNewCommand("cats", "Prints a cute cat!");

        AddNewCommandWithOption("register",
            "registers an user profile manually",
            "userid",
            "what discord ID do you want to register?"
            );

        // Command for eliminating a player's profile
        AddNewCommandWithOption("terminate",
            "deletes a player profile, completely",
            "userid",
            "which user do you want to terminate?"
            );
        */
        Log.WriteLine("Done preparing the commands.");

        return;
    }

    public static InterfaceCommand GetCommandInstance(string _commandName)
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