using Discord;
using Discord.WebSocket;

public static class CategoryAndChannelManager
{
    public static async Task CreateCategoriesAndChannelsForTheDiscordServer()
    {
        Log.WriteLine("Starting to create categories and channels for the discord server");

        await GenerateRegularCategories();

        Log.WriteLine("Done looping through the category names.");
    }

    private static async Task GenerateRegularCategories()
    {
        foreach (CategoryType categoryName in Enum.GetValues(typeof(CategoryType)))
        {
            Log.WriteLine("Looping on category name: " + categoryName);

            await GenerateCategory(categoryName, categoryName);
        }
    }

    private static async Task GenerateCategory(CategoryType _categoryType, Enum _categoryName)
    {
        try
        {
            Log.WriteLine("Generating category named: " + _categoryType);

            InterfaceCategory interfaceCategory = GetCategoryInstance(_categoryType);
            if (interfaceCategory == null)
            {
                Log.WriteLine(nameof(interfaceCategory).ToString() + " was null!", LogLevel.CRITICAL);
                return;
            }

            Log.WriteLine("interfaceCategory name: " + interfaceCategory.CategoryType, LogLevel.DEBUG);

            string finalCategoryName = EnumExtensions.GetEnumMemberAttrValue(_categoryName);
            Log.WriteLine("Category name is: " + interfaceCategory.CategoryType);

            SocketCategoryChannel? socketCategoryChannel = FindOrCreateSocketCategoryChannel(interfaceCategory, finalCategoryName);
            if (socketCategoryChannel == null)
            {
                Log.WriteLine(nameof(socketCategoryChannel).ToString() + " was null!", LogLevel.CRITICAL);
                return;
            }

            SocketRole? role = await RoleManager.CheckIfRoleExistsByNameAndCreateItIfItDoesntElseReturnIt(finalCategoryName);
            if (role == null)
            {
                Log.WriteLine(nameof(role).ToString() + " was null!", LogLevel.CRITICAL);
                return;
            }

            if (Database.Instance.Categories.FindIfInterfaceCategoryExistsWithCategoryId(socketCategoryChannel.Id))
            {
                interfaceCategory = Database.Instance.Categories.FindInterfaceCategoryWithCategoryId(socketCategoryChannel.Id);
            }

            await interfaceCategory.CreateChannelsForTheCategory(socketCategoryChannel.Id, role);
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.CRITICAL);
        }
    }

    private static SocketCategoryChannel? FindOrCreateSocketCategoryChannel(InterfaceCategory _interfaceCategory, string _finalCategoryName)
    {
        bool contains = false;

        foreach (var ct in Database.Instance.Categories.CreatedCategoriesWithChannels)
        {
            if (ct.Value.CategoryType == _interfaceCategory.CategoryType)
            {
                contains = CategoryRestore.CheckIfCategoryHasBeenDeletedAndRestoreForCategory(ct.Key);
                if (contains)
                {
                    break;
                }
            }
        }

        if (contains)
        {
            InterfaceCategory? dbCategory = Database.Instance.Categories.FindInterfaceCategoryByCategoryName(_interfaceCategory.CategoryType);
            if (dbCategory == null)
            {
                Log.WriteLine(nameof(dbCategory).ToString() + " was null!", LogLevel.CRITICAL);
                return null;
            }

            var guild = BotReference.GetGuildRef();
            SocketCategoryChannel? socketCategoryChannel = guild.GetCategoryChannel(dbCategory.SocketCategoryChannelId);
            if (socketCategoryChannel == null)
            {
                Log.WriteLine(nameof(socketCategoryChannel).ToString() + " was null!", LogLevel.CRITICAL);
                return null;
            }

            return socketCategoryChannel;
        }
        else
        {
            SocketRole? role = RoleManager.CheckIfRoleExistsByNameAndCreateItIfItDoesntElseReturnIt(_finalCategoryName).Result;
            if (role == null)
            {
                Log.WriteLine(nameof(role).ToString() + " was null!", LogLevel.CRITICAL);
                return null;
            }

            SocketCategoryChannel? socketCategoryChannel =
                _interfaceCategory.CreateANewSocketCategoryChannelAndReturnIt(_finalCategoryName, role).Result;
            if (socketCategoryChannel == null)
            {
                Log.WriteLine(nameof(socketCategoryChannel).ToString() + " was null!", LogLevel.CRITICAL);
                return null;
            }

            Database.Instance.Categories.AddToCreatedCategoryWithChannelWithUlongAndInterfaceCategory(socketCategoryChannel.Id, _interfaceCategory);
            return socketCategoryChannel;
        }
    }

    private static InterfaceCategory? GetCategoryInstance(CategoryType _categoryType)
    {
        try
        {
            return (InterfaceCategory)EnumExtensions.GetInstance(_categoryType.ToString());
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.CRITICAL);
            throw new InvalidOperationException(ex.Message);
        }
    }
}