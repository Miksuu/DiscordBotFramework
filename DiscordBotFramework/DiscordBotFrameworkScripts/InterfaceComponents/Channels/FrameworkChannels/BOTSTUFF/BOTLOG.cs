﻿using Discord;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using Discord.WebSocket;

[DataContract]
public class BOTLOG : BaseChannel
{
    public BOTLOG()
    {
        thisInterfaceChannel.ChannelType = ChannelType.BOTLOG;
    }

    public override List<Overwrite> GetGuildPermissions(
        SocketRole _role, params ulong[] _allowedUsersIdsArray)
    {
        return new List<Overwrite>
        {
        };
    }

    /*
    public override Task PrepareChannelMessages()
    {
        Log.WriteLine("Setting logging channel id with: " + channelId);
        return Task.CompletedTask;
    }*/
}