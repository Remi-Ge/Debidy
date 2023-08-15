using Discord;
using System.Text.RegularExpressions;

public static class Program
{
    public static FilesStorage filesStorage;
    public static Settings settings;
    public static DiscordBot discordBot;

    public static string programVersion = "1.0.0";

    public static FileSender fileSender;
    private static void Main(string[] args)
    {
        Console.WriteLine($"Welcome to Debidy {programVersion}, a piece of software to share big files with Discord!");
        Console.WriteLine();

        filesStorage = new FilesStorage("Debidy"); //Debidy
        settings = new Settings();

        //when opened for the first time
        if (filesStorage.FirstTimeOpened())
        {
            Console.WriteLine("You opened Debidy for the first time!");
            Console.WriteLine();
        }
        //not the first time
        else
        {
            Console.WriteLine("It's not your first time opening Debidy, If you want to change the settings" +
                              @" just open C:\Users\USER(replace with your user)\AppData\Roaming\RemiDv\Debidy\settings.json");
            Console.WriteLine("If you modify something incorrectly, no worries, when you will open the program, it will ask you to reconfigure");
            Console.WriteLine();
        }

        filesStorage.CreateAllDirectories();
        filesStorage.ClearCache();

        if (!settings.IsSettingFileValid())
        {
            Console.WriteLine("The Settings file doesn't exist or is corrupted! ");
            Console.WriteLine();
            settings.NewConfiguration();
        }

        settings.ReloadSettingsFile();

        discordBot = new DiscordBot(settings.configuration.discordToken);
        discordBot.RunBotAsync().GetAwaiter().GetResult();
    }

    public static async Task<string> GetGithubText(string githubUrl)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                string fileContent = await httpClient.GetStringAsync(githubUrl);
                return fileContent;
            }
            catch (Exception ex)
            {
                return "Error while reading the message : " + ex.Message;
            }
        }
    }

    public static async Task<string> CheckNewUpdates()
    {
        string lastestVersion =
            await GetGithubText(
                "https://raw.githubusercontent.com/Remi-Dv/informations/main/DownloadDiscordBot/LastVersion.txt");
        lastestVersion = Regex.Replace(lastestVersion, "[^0-9.]", "");
        if (lastestVersion != programVersion)
        {
            string fileUrl = await GetGithubText("https://raw.githubusercontent.com/Remi-Dv/informations/main/DownloadDiscordBot/DownloadUrl");
            return $"There is a NEW VERSION: {programVersion} -> {lastestVersion}\n" +
                   $"Download it at {fileUrl}";
        }
        else
        {
            return $"You are using the lastest version of Debidy. Current version: " +
                $"{programVersion} Latest version: {lastestVersion}";
        }
    }
}