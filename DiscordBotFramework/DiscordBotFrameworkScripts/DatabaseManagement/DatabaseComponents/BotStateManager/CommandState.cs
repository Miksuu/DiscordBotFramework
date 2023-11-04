using System.Runtime.Serialization;

[DataContract]
public enum CommandState
{
    Active,
    Serializing,
    AwaitingResponse,
}