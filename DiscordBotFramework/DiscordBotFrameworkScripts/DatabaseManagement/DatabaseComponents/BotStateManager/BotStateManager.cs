using Discord.WebSocket;
using System.Runtime.Serialization;
using System.Collections.Concurrent;

[DataContract]
public class BotStateManager
{
    public ConcurrentBag<ActiveCommand> CommandsBeingProcessed
    {
        get => commandsBeingProcessed.GetValue();
        set => commandsBeingProcessed.SetValue(value);
    }

    [DataMember] public logConcurrentBag<ActiveCommand> commandsBeingProcessed = new logConcurrentBag<ActiveCommand>();

    public void AddCommand(ActiveCommand _command)
    {
        CommandsBeingProcessed.Add(_command);
    }
    public void RemoveCommand(ActiveCommand _command)
    {
        CommandsBeingProcessed.TryTake(out _command);
    }

    public ActiveCommand GetCommand(ActiveCommand _command)
    {
        if (!CommandsBeingProcessed.Contains(_command))
        {
            return null;
        }

        return _command;
    }
}