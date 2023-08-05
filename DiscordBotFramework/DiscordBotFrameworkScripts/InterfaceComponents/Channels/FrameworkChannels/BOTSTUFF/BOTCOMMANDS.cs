using Discord;
using System.Data;
using System;
using System.Runtime.Serialization;
using Discord.WebSocket;
using System.Collections.Concurrent;

[DataContract]
public class BOTCOMMANDS : BaseChannel
{
    public BOTCOMMANDS()
    {
        thisInterfaceChannel.ChannelType = ChannelType.BOTCOMMANDS;
    }

    public override List<Overwrite> GetGuildPermissions(SocketRole _role, params ulong[] _allowedUsersIdsArray)
    {
        return new List<Overwrite>
        {
        };
    }

    public override Task<bool> HandleChannelSpecificGenerationBehaviour()
    {
        return Task.FromResult(false);
    }
}