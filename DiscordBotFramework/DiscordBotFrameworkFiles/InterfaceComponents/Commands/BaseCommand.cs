﻿using Discord.WebSocket;
using Discord;

public abstract class BaseCommand : InterfaceCommand
{
    CommandName InterfaceCommand.CommandName
    {
        get
        {
            Log.WriteLine("Getting " + nameof(commandName) + ": " +
                commandName);
            return commandName;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(commandName) + commandName
                + " to: " + value);
            commandName = value;
        }
    }

    string InterfaceCommand.CommandDescription
    {
        get
        {
            Log.WriteLine("Getting " + nameof(commandDescription) + ": " +
                commandOption);
            return commandDescription;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(commandOption) + commandOption
                + " to: " + value);
            commandDescription = value;
        }
    }

    CommandOption InterfaceCommand.CommandOption
    {
        get
        {
            Log.WriteLine("Getting " + nameof(commandOption) + ": " +
                commandOption);
            return commandOption;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(commandOption) + commandOption
                + " to: " + value);
            commandOption = value;
        }
    }

    protected CommandName commandName;
    protected string commandDescription = "";
    protected CommandOption commandOption;
    protected bool isAdminCommand = false;

    public async Task<Response> ReceiveCommandAndCheckForAdminRights(
        SocketSlashCommand _command, string _firstOptionString)
    {
        ulong commandSenderId = _command.User.Id;

        Log.WriteLine("Received command " + commandName + " by: " + commandSenderId);

        bool senderIsAdmin = Database.Instance.Admins.CheckIfCommandSenderWasAnAdmin(_command);

        if (isAdminCommand && senderIsAdmin)
        {
            Log.WriteLine("Command was admin command and the sender was admin");
            return await ActivateCommandFunction(_command, _firstOptionString);
        }
        else if (isAdminCommand && !senderIsAdmin)
        {
            Log.WriteLine(commandSenderId + " tried to access an admin command");
            return new Response("You are not allowed to use that command!", false);
        }

        Log.WriteLine("Command was a regular one");

        return await ActivateCommandFunction(_command, _firstOptionString);
    }

    protected abstract Task<Response> ActivateCommandFunction(
        SocketSlashCommand _command, string _firstOptionString);

    public async Task AddNewCommandWithOption()
    {
        if (commandOption == null)
        {
            Log.WriteLine(nameof(commandOption) + " was null!", LogLevel.CRITICAL);
            return;
        }

        Log.WriteLine("Installing a command: " + commandName.ToString() + " | with description: " +
            commandDescription + " | that has an option with name: " + commandOption.OptionName +
            " | and optionDescription: " + commandOption.OptionDescription, LogLevel.DEBUG);

        var guildCommand = new Discord.SlashCommandBuilder()
            .WithName(commandName.ToString().ToLower())
            .WithDescription(commandDescription).AddOption(
            commandOption.OptionName, ApplicationCommandOptionType.String,
            commandOption.OptionDescription, isRequired: true);

        Log.WriteLine("Starting to build a guild command: " + commandName, LogLevel.DEBUG);

        var builtCommand = guildCommand.Build();

        Log.WriteLine("Done building a guild command: " + commandName, LogLevel.DEBUG);

        var client = BotReference.GetClientRef();

        await client.Rest.CreateGuildCommand(
            builtCommand, Preferences.Instance.GuildID);

        Log.WriteLine("Done creating a command with option: " + guildCommand.Name, LogLevel.DEBUG);
    }
}