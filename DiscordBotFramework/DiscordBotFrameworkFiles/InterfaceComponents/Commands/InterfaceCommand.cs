﻿using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

[JsonObjectAttribute]
public interface InterfaceCommand
{
    public CommandName CommandName{ get; set; }
    public string CommandDescription { get; set; }
    public CommandOption CommandOption { get; set; }
    public Task<Response> ReceiveCommandAndCheckForAdminRights(
        SocketSlashCommand _command, string _firstOptionString);
    public Task AddNewCommandWithOption();
}