using Newtonsoft.Json;

public class Settings
{
    public string discordToken;
    public ulong otherBotId;
    public ulong botAdminId;
    public ulong channelId;
    public long maxSizeMb;
    public int chunkSizeMb;

    public Setting configuration;

    public bool NewConfiguration()
    {
        Console.WriteLine("What is your Discord Id? (Settings>Advanced>enable developper mode," +
            "then right click on a profile to get his Id)");
        string userInput;
        while (true)
        {
            userInput = Console.ReadLine();

            if (ulong.TryParse(userInput, out botAdminId))
            {
                Console.WriteLine();
                Console.WriteLine("Great! Your discord Id is " + botAdminId + ". (restart the app if not)");
                break;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Invalid input! Please enter a valid Discord Id.");
            }
        }

        Console.WriteLine();
        Console.WriteLine("What is the Discord channel Id that will be used to send files? (Settings>Advanced>enable developper mode," +
            "then right click on a channel to get his Id)");

        userInput = null;
        while (true)
        {
            userInput = Console.ReadLine();

            if (ulong.TryParse(userInput, out channelId))
            {
                Console.WriteLine();
                Console.WriteLine("Great! Your discord channel Id is " + channelId + ". (restart the app if not)");
                break;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Invalid input! Please enter a valid Discord channel Id.");
            }
        }

        Console.WriteLine("You know need to create a discord Bot, (searh on internet if you don't know how)");
        Console.WriteLine("WARNING: Make sure to enable all Privileged Gateway Intents (on the Bot section on your application)");

        Console.WriteLine();
        Console.WriteLine("What is your Discord bot token? (Go to the developper portal on your web browser, " +
            "on the section with your bot, you can reset the token)");

        userInput = null;
        userInput = Console.ReadLine();

        discordToken = userInput;

        Console.WriteLine();
        Console.WriteLine("Great! Your Bot token is " + discordToken + ". (restart the app if not)");

        Console.WriteLine();
        Console.WriteLine("Almost Done! Please enter now the Id of your partner discord bot that will be used.");

        userInput = null;
        while (true)
        {
            userInput = Console.ReadLine();

            if (ulong.TryParse(userInput, out otherBotId))
            {
                Console.WriteLine();
                Console.WriteLine("Great! Your the other bot Id is " + otherBotId + ". (restart the app if not)");
                break;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Invalid input! Please enter a valid Discord bot Id.");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Now, Enter the maxSize of a filePart in Mb. " +
            "(Usually put 20 but put less if you have a bad internet speed)");

        userInput = null;
        while (true)
        {
            userInput = Console.ReadLine();

            if (int.TryParse(userInput, out chunkSizeMb) && chunkSizeMb <= 25)
            {
                Console.WriteLine();
                Console.WriteLine("Great! the max file part size is " + chunkSizeMb + "Mb. (restart the app if not)");
                break;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Invalid input! Please enter a valid size in Mb.");
            }
        }

        Console.WriteLine();
        Console.WriteLine("And the final Parameter: How much size in Mb will be all of the files shared by the other user? (0 for unlimited, 1000 is 1 Gb)");

        userInput = null;
        while (true)
        {
            userInput = Console.ReadLine();

            if (long.TryParse(userInput, out maxSizeMb))
            {
                Console.WriteLine();
                Console.WriteLine("Great! the max size of all the files shared by the other is " + maxSizeMb + "Mb. (restart the app if not)");
                break;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Invalid input! Please enter a valid number of Mb.");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Discord token: " + discordToken);
        Console.WriteLine("Your discord Id: " + botAdminId);
        Console.WriteLine("The other bot Id: " + otherBotId);
        Console.WriteLine("The channel Id: " + channelId);
        Console.WriteLine("Max size of a part: " + chunkSizeMb + "Mb");
        Console.WriteLine("Max storage for the downloads: " + maxSizeMb + "Mb");

        Console.WriteLine();
        Console.WriteLine("Is everythings Ok? Press Enter if yes (restart the app if not)");

        Console.ReadLine();

        SaveSettings(discordToken, otherBotId, botAdminId, channelId, maxSizeMb, chunkSizeMb);

        return true;
    }

    private void SaveSettings(string _discordToken, ulong _otherBotId
        , ulong _botAdminId, ulong _channelId, long _maxSizeMb, int _chunkSizeMb)
    {
        Setting setting = new Setting()
        {
            discordToken = _discordToken,
            otherBotId = _otherBotId,
            botAdminId = _botAdminId,
            channelId = _channelId,
            maxSizeMb = _maxSizeMb,
            chunkSizeMb = _chunkSizeMb
        };

        try
        {
            string jsonSetting = JsonConvert.SerializeObject(setting, Formatting.Indented);
            File.WriteAllText(Program.filesStorage.settingsFilePath, jsonSetting);
        }
        catch (Exception ex)
        {
            
        }
    }

    public bool IsSettingFileValid()
    {
        if (!File.Exists(Program.filesStorage.settingsFilePath))
        {
            return false;
        }

        try
        {
            Setting setting = JsonConvert.DeserializeObject<Setting>
                (File.ReadAllText(Program.filesStorage.settingsFilePath));
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public bool ReloadSettingsFile()
    {
        if (IsSettingFileValid())
        {
            configuration = JsonConvert.DeserializeObject<Setting>
                (File.ReadAllText(Program.filesStorage.settingsFilePath));
            return true;
        }

        return false;
    }

    public class Setting
    {
        public string discordToken;
        public ulong otherBotId;
        public ulong botAdminId;
        public ulong channelId;
        public long maxSizeMb;
        public int chunkSizeMb;
    }
}