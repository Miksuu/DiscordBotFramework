using Discord.WebSocket;
using System.Diagnostics;

public static class ButtonHandler
{
    public static async Task HandleButtonPress(SocketMessageComponent _component)
    {
        try
        {
            Log.WriteLine("Button press detected by: " + _component.User.Id);

            ulong componentChannelId = _component.Channel.Id;
            ulong componentMessageId = _component.Message.Id;

            var category = Database.GetInstance<DiscordBotDatabase>().Categories.FindInterfaceCategoryWithChannelId(componentChannelId);
            var interfaceMessage = category.FindInterfaceChannelWithIdInTheCategory(
                componentChannelId).FindInterfaceMessageWithIdInTheChannel(
                    componentMessageId);

            var button = interfaceMessage.FindButtonWithComponentDataCustomIdInTheMessage(
                                _component.Data.CustomId);

            button.CallButtonActivation(_component, interfaceMessage);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("error 50006"))
            {
                Log.WriteLine("skipped empty message try-catch");
                return;
            }

            Log.WriteLine(ex.Message, LogLevel.ERROR);
            return;
        }
    }
}