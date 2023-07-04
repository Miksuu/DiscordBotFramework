﻿using Discord;
using Discord.WebSocket;
using System;


public static class CategoryRestore
{
    public static bool CheckIfCategoryHasBeenDeletedAndRestoreForCategory(ulong _categoryKey)
    {
        Log.WriteLine("Checking if categoryId: " + _categoryKey +
            " has been deleted.");

        var guild = BotReference.GetGuildRef();

        if (guild.CategoryChannels.Any(x => x.Id == _categoryKey))
        {
            Log.WriteLine("Category found, returning. ");
            return true;
        }

        Log.WriteLine("Category " + _categoryKey +
            " not found, regenerating it...", LogLevel.DEBUG);

        // Delete the old entry from the database
        //Database.Instance.Categories.RemoveFromCreatedCategoryWithChannelWithKey(
        //    _categoryKey);

        return false;
    }

    public static bool CheckIfLeagueCategoryHasBeenDeletedAndRestoreForCategory(
        ulong _categoryKey, string _categoryName)
    {
        Log.WriteLine("Checking if categoryId: " + _categoryKey +
            " has been deleted.");

        var guild = BotReference.GetGuildRef();

        if (guild.CategoryChannels.Any(x => x.Name == _categoryName))
        {
            Log.WriteLine("Category found, returning. ");
            return true;
        }

        Log.WriteLine("Category " + _categoryKey +
            " not found, regenerating it...", LogLevel.DEBUG);

        // Delete the old entry from the database
        //Database.Instance.Categories.RemoveFromCreatedCategoryWithChannelWithKey(
        //    _categoryKey);

        return false;
    }
}