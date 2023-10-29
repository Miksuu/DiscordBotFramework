public class MessageComponents
{
    public MessageComponents(){}

    public MessageComponents(string _message)
    {
        message = _message;
        playersToMention = "";
    }

    public MessageComponents(string _message, string _playersToMention)
    {
        message = _message;
        playersToMention = _playersToMention;
    }

    public string message { get; set; }
    public string playersToMention { get; set; }
}