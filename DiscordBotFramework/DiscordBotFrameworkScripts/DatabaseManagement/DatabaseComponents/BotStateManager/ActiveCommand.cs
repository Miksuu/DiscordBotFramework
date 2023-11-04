using Discord.WebSocket;
using System.Runtime.Serialization;
using System.Collections.Concurrent;

[DataContract]
public class ActiveCommand
{
    public string CommandData
    {
        get => commandData.GetValue();
        set => commandData.SetValue(value);
    }

    public CommandState CommandState
    {
        get => commandState.GetValue();
        set => commandState.SetValue(value);
    }

    [DataMember]
    public logVar<string> commandData = new logVar<string>();

    [DataMember]
    public logEnum<CommandState> commandState = new logEnum<CommandState>();
}