using Discord.WebSocket;
using Discord;
using Newtonsoft.Json;
using System.Collections.Concurrent;

[JsonObjectAttribute]
public interface InterfaceChannel
{
    public ChannelType ChannelType { get; set; }
    public string ChannelName { get; set; }
    public ulong ChannelId { get; set; }
    public ulong ChannelsCategoryId { get; set; }
    public ConcurrentDictionary<MessageName, bool> ChannelMessages { get; set; }
    public ConcurrentDictionary<ulong, InterfaceMessage> InterfaceMessagesWithIds { get; set; }

    public abstract List<Overwrite> GetGuildPermissions(
        SocketRole _role, params ulong[] _allowedUsersIdsArray);

    public Task CreateAChannelForTheCategory(
        SocketRole _role, ulong _channelCategoryId, params ulong[] _allowedUsersIdsArray);
    public Task CreateAChannelForTheCategoryWithoutRole(params ulong[] _allowedUsersIdsArray);
    public Task<InterfaceMessage> CreateAMessageForTheChannelFromMessageName(
        MessageName _MessageName, bool _displayMessage = true,
        SocketMessageComponent? _component = null, bool _ephemeral = true);
    public Task<Discord.IUserMessage> CreateARawMessageForTheChannelFromMessageName(
        string _input, string _embedTitle = "", bool _displayMessage = true,
        SocketMessageComponent? _component = null, bool _ephemeral = true, params string[] _files);
    public Task<InterfaceMessage> CreateARawMessageForTheChannelFromMessageNameWithAttachmentData(
        string _input, AttachmentData[] _attachmentDatas, string _embedTitle = "", bool _displayMessage = true,
        SocketMessageComponent? _component = null, bool _ephemeral = true);

    public Task PostChannelMessages();
    public Task<bool> HandleChannelSpecificGenerationBehaviour();

    public InterfaceMessage FindInterfaceMessageWithNameInTheChannel(
        MessageName _messageName);

    public InterfaceMessage FindInterfaceMessageWithIdInTheChannel(ulong _messageId);
    public void FindInterfaceMessageWithNameInTheChannelAndUpdateItIfItExists(
        MessageName _messageName);

    public Task<IMessageChannel> GetMessageChannelById();
    public Task<string> DeleteMessagesInAChannelWithMessageName(MessageName _messageNameToDelete);
    public Task DeleteThisChannel(
        InterfaceCategory _interfaceCategory, InterfaceChannel _interfaceChannel, string _nameMustContain);
    public bool CheckIfChannelHasBeenDeletedAndRestoreForCategory(ulong _categoryId);
}