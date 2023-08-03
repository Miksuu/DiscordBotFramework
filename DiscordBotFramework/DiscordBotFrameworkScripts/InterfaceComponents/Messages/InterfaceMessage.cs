﻿using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Collections.Concurrent;

[JsonObjectAttribute]   
public interface InterfaceMessage
{
    public MessageName MessageName { get; set; }
    public ConcurrentDictionary<ButtonName, int> MessageButtonNamesWithAmount { get; set; }

    // Embed properties
    public string MessageEmbedTitle { get; set; }
    public string MessageDescription { get; set; }
    public Discord.Color MessageEmbedColor { get; set; }
    public ulong MessageId { get; set; }
    public ulong MessageChannelId { get; set; }
    public ulong MessageCategoryId { get; set; }
    public ConcurrentBag<InterfaceButton> ButtonsInTheMessage { get; set; }

    public Discord.IUserMessage CachedUserMessage { get; set; }

    public Task<InterfaceMessage> CreateTheMessageAndItsButtonsOnTheBaseClass(
        InterfaceChannel _interfaceChannel, bool _embed,
        bool _displayMessage = true, ulong _channelCategoryId = 0,
        SocketMessageComponent? _component = null, bool _ephemeral = true,
        params string[] _files);

    public Task<InterfaceMessage> CreateTheMessageAndItsButtonsOnTheBaseClassWithAttachmentData(
        InterfaceChannel _interfaceChannel, AttachmentData[] _attachmentDatas,
        bool _displayMessage = true, ulong _channelCategoryId = 0,
        SocketMessageComponent? _component = null, bool _ephemeral = true);
    public void ModifyMessage(string _newContent);
    public Task AddContentToTheEndOfTheMessage(string _content);
    public abstract Task<string> GenerateMessage(ulong _messageCategoryId = 0);
    public void GenerateAndModifyTheMessage(ulong _messageCategoryId = 0);
    public Task<Discord.IMessage> GetMessageById(IMessageChannel _channel);

    public InterfaceButton FindButtonWithComponentDataCustomIdInTheMessage(string _componentDataCustomId);
    public abstract string GenerateMessageFooter();
}