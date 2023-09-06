using Discord;
using Discord.WebSocket;
using System.ComponentModel;
using System.Data;
using System.Reflection.Metadata.Ecma335;

public static class CategoryAndChannelManager
{
    public static async Task CreateCategoriesAndChannelsForTheDiscordServer()
    {
        Log.WriteLine("Starting to create categories and channels for the discord server", LogLevel.DEBUG);

        await GenerateRegularCategories();

        Log.WriteLine("Done looping through all the category names.", LogLevel.DEBUG);
    }

    private static async Task GenerateRegularCategories()
    {
        Log.WriteLine("Starting to generate RegularCategories", LogLevel.DEBUG);

        foreach (CategoryType categoryType in Enum.GetValues(typeof(CategoryType)))
        {
            try
            {
                Log.WriteLine("Looping on category name: " + categoryType);

                InterfaceCategory interfaceCategory = GetCategoryInstance(categoryType);

                if (interfaceCategory.SkipOnRegularCategoryGeneration)
                {
                    Log.WriteLine("skipped " + categoryType);
                    continue;
                }

                await GenerateCategoryAndItsChannels(categoryType);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message);
                continue;
            }
        }

        Log.WriteLine("Done generating RegularCategories", LogLevel.DEBUG);
    }

    private static async Task GenerateCategoryAndItsChannels(CategoryType _categoryType)
    {
        try
        {
            Log.WriteLine("Generating category named: " + _categoryType + " with enum: " + _categoryType, LogLevel.DEBUG);

            InterfaceCategory interfaceCategory = GetCategoryInstance(_categoryType);

            Log.WriteLine("interfaceCategory name: " + interfaceCategory.CategoryType, LogLevel.DEBUG);

            string finalCategoryName = EnumExtensions.GetEnumMemberAttrValue(_categoryType);
            Log.WriteLine("Category name is: " + finalCategoryName);

            ulong socketCategoryChannelId = await FindOrCreateSocketCategoryChannelAndReturnId(interfaceCategory, finalCategoryName);

            Log.WriteLine("id: " + socketCategoryChannelId);

            SocketRole role = await RoleManager.CheckIfRoleExistsByNameAndCreateItIfItDoesntElseReturnIt(finalCategoryName);

            if (DiscordBotDatabase.Instance.Categories.FindIfInterfaceCategoryExistsWithCategoryId(socketCategoryChannelId))
            {
                interfaceCategory = DiscordBotDatabase.Instance.Categories.FindInterfaceCategoryWithCategoryId(socketCategoryChannelId);
            }

            await interfaceCategory.CreateChannelsForTheCategory(socketCategoryChannelId, role);

            Log.WriteLine("Done with generating category: " + _categoryType + " with enum: " + _categoryType, LogLevel.DEBUG);
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }

    public static async Task<InterfaceCategory> GenerateCategoryWithoutItsChannels(CategoryType _categoryType, Enum _categoryName)
    {
        try
        {
            Log.WriteLine("Generating category named: " + _categoryType + " with enum: " + _categoryName, LogLevel.DEBUG);

            InterfaceCategory interfaceCategory = GetCategoryInstance(_categoryType);

            Log.WriteLine("interfaceCategory name: " + interfaceCategory.CategoryType, LogLevel.DEBUG);

            string finalCategoryName = EnumExtensions.GetEnumMemberAttrValue(_categoryName);
            Log.WriteLine("Category name is: " + finalCategoryName);

            ulong socketCategoryChannelId = await FindLeagueAndCreateSocketCategoryChannelAndReturnId(interfaceCategory, finalCategoryName);

            Log.WriteLine("id: " + socketCategoryChannelId);

            //SocketRole role = await RoleManager.CheckIfRoleExistsByNameAndCreateItIfItDoesntElseReturnIt(finalCategoryName);

            if (DiscordBotDatabase.Instance.Categories.FindIfInterfaceCategoryExistsWithCategoryId(socketCategoryChannelId))
            {
                interfaceCategory = DiscordBotDatabase.Instance.Categories.FindInterfaceCategoryWithCategoryId(socketCategoryChannelId);
            }

            Log.WriteLine("Done with generating category: " + _categoryType + " with enum: " + _categoryName, LogLevel.DEBUG);

            interfaceCategory.SocketCategoryChannelId = socketCategoryChannelId;

            return interfaceCategory;
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }

    private static bool FindCategoryAndCheckIfItHasBeenDeletedAndRestoreForCategory(CategoryType _categoryType)
    {
        Log.WriteLine("Looking for: " + _categoryType, LogLevel.DEBUG);

        foreach (var categoryKvp in DiscordBotDatabase.Instance.Categories.CreatedCategoriesWithChannels)
        {
            try
            {
                Log.WriteLine("Looping on: " + categoryKvp.Key + " | " + categoryKvp.Value.CategoryType);
                if (categoryKvp.Value.CategoryType == _categoryType)
                {
                    Log.WriteLine("Equals: " + _categoryType);
                    if (categoryKvp.Value.CheckIfCategoryHasBeenDeletedAndRestoreForCategory(categoryKvp.Key))
                    {
                        Log.WriteLine("Found: " + categoryKvp.Key + " | " + categoryKvp.Value.CategoryType, LogLevel.DEBUG);
                        return true;
                    }
                }
                Log.WriteLine("Did not find with: " + categoryKvp.Key + " | " + categoryKvp.Value.CategoryType);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message, LogLevel.ERROR);
                continue;
            }
        }

        Log.WriteLine("Didn't find: " + _categoryType, LogLevel.DEBUG);

        return false;
    }

    private async static Task<ulong> FindOrCreateSocketCategoryChannelAndReturnId(InterfaceCategory _interfaceCategory, string _finalCategoryName)
    {
        try
        {
            Log.WriteLine("Finding with: " + _interfaceCategory + "| final name:" + _finalCategoryName, LogLevel.DEBUG);

            if (FindCategoryAndCheckIfItHasBeenDeletedAndRestoreForCategory(_interfaceCategory.CategoryType))
            {
                Log.WriteLine("True with: " + _interfaceCategory.CategoryType);

                InterfaceCategory dbCategory = DiscordBotDatabase.Instance.Categories.FindInterfaceCategoryByCategoryName(_interfaceCategory.CategoryType);

                Log.WriteLine("Found dbCategory: " + dbCategory.CategoryType);

                var guild = BotReference.GetGuildRef();
                ulong socketCategoryChannelId = guild.GetCategoryChannel(dbCategory.SocketCategoryChannelId).Id;
                Log.WriteLine("returning found id: " + socketCategoryChannelId, LogLevel.DEBUG);
                return socketCategoryChannelId;
            }
            else
            {
                Log.WriteLine("false with: " + _interfaceCategory.CategoryType);

                SocketRole role = await RoleManager.CheckIfRoleExistsByNameAndCreateItIfItDoesntElseReturnIt(_finalCategoryName);

                Log.WriteLine("Role: " + role.Id + " | name: " + role.Name);

                var socketCategoryChannelId =
                    await _interfaceCategory.CreateANewSocketCategoryChannelAndReturnItAsId(_finalCategoryName, role);

                Log.WriteLine("found id: " + socketCategoryChannelId);

                DiscordBotDatabase.Instance.Categories.AddToCreatedCategoryWithChannelWithUlongAndInterfaceCategory(socketCategoryChannelId, _interfaceCategory);

                Log.WriteLine("Added, returning found id: " + socketCategoryChannelId, LogLevel.DEBUG);
                return socketCategoryChannelId;
            }
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }

    private async static Task<ulong> FindLeagueAndCreateSocketCategoryChannelAndReturnId(InterfaceCategory _interfaceCategory, string _finalCategoryName)
    {
        try
        {
            Log.WriteLine("Finding with: " + _interfaceCategory + "| final name:" + _finalCategoryName, LogLevel.DEBUG);

            Log.WriteLine("false with: " + _interfaceCategory.CategoryType);

            SocketRole role = await RoleManager.CheckIfRoleExistsByNameAndCreateItIfItDoesntElseReturnIt(_finalCategoryName);

            Log.WriteLine("Role: " + role.Id + " | name: " + role.Name);

            var socketCategoryChannelId =
                await _interfaceCategory.CreateANewSocketCategoryChannelAndReturnItAsId(_finalCategoryName, role);

            Log.WriteLine("found id: " + socketCategoryChannelId);

            DiscordBotDatabase.Instance.Categories.AddToCreatedCategoryWithChannelWithUlongAndInterfaceCategory(socketCategoryChannelId, _interfaceCategory);

            Log.WriteLine("Added, returning found id: " + socketCategoryChannelId, LogLevel.DEBUG);
            return socketCategoryChannelId;
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }

    private static InterfaceCategory GetCategoryInstance(CategoryType _categoryType)
    {
        Log.WriteLine("Getting instance: " + _categoryType, LogLevel.DEBUG);
        try
        {
            return (InterfaceCategory)EnumExtensions.GetInstance(_categoryType.ToString());
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }
}