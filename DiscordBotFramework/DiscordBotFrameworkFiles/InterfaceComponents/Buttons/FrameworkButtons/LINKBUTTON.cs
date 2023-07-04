﻿using Discord;
using System.Data;
using System;
using System.Runtime.Serialization;
using Discord.WebSocket;
using System.Threading.Channels;

[DataContract]
public class LINKBUTTON : BaseButton
{
    public LINKBUTTON()
    {
        buttonName = ButtonName.LINKBUTTON;
        thisInterfaceButton.ButtonLabel = "Link";
        buttonStyle = ButtonStyle.Link;
        ephemeralResponse = true;
    }

    protected override string GenerateCustomButtonProperties(int _buttonIndex, ulong _leagueCategoryId)
    {
        return "";
    }

    public override Task<Response> ActivateButtonFunction(
        SocketMessageComponent _component, InterfaceMessage _interfaceMessage)
    {
        return Task.FromResult(new Response("Not implemented yet!", false));
    }

    public Discord.ButtonBuilder CreateALinkButton(AttachmentData _attachmentData)
    {
        Log.WriteLine("Creating a link button: " + _attachmentData.attachmentName + " | " +
            _attachmentData.attachmentLink);

        var button = new Discord.ButtonBuilder()
        {
            Label = _attachmentData.attachmentName,
            Url = _attachmentData.attachmentLink,
            Style = buttonStyle,
        };

        return button;
    }
}