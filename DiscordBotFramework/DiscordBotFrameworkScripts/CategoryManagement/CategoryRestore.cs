﻿using Discord;
using Discord.WebSocket;
using System;


public static class CategoryRestore
{
    public static bool CheckIfLeagueCategoryHasBeenDeletedAndRestoreForCategory(
        ulong _categoryKey, SocketGuild _guild, string _categoryName)
    {
        Log.WriteLine("Checking if categoryId: " + _categoryKey +
            " has been deleted.");

        if (_guild.CategoryChannels.Any(x => x.Name == _categoryName))
        {
            Log.WriteLine("Category found, returning. ");
            return true;
        }

        Log.WriteLine("Category " + _categoryKey +
            " not found, regenerating it...", LogLevel.DEBUG);

        // Delete the old entry from the database
        //Database.GetInstance<DiscordBotDatabase>().Categories.RemoveFromCreatedCategoryWithChannelWithKey(
        //    _categoryKey);

        return false;
    }
}