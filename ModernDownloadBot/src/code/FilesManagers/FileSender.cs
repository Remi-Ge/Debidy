using Discord.Commands;
using Discord.WebSocket;
using System.Diagnostics;
using System.Net.NetworkInformation;

public class FileSender
{
    public bool stop = false;

    public List<string> missingFiles = new List<string>();

    public string filePath;

    public SocketTextChannel requestChannel;

    public string cacheDirectoryPath;

    private int chunkIndex = 0;


    public FileSender(string _filePath, SocketTextChannel _requestChannel)
    {
        filePath =_filePath;
        requestChannel = _requestChannel;
        Task.Run(Initialize);
    }

    ~FileSender()
    {

    }

    private async Task Initialize()
    {
        cacheDirectoryPath = Path.Combine
            (Program.filesStorage.filePartsSendingPath, Path.GetFileName(filePath).Replace(" ", "_"));
        Program.filesStorage.CreateDirectory(cacheDirectoryPath);

        var message = await requestChannel.SendMessageAsync("Sending 0.00%");

        var channel = Program.discordBot.client.GetChannel(Program.settings.configuration.channelId) as ISocketMessageChannel;

        int chunkSizeMb = Program.settings.configuration.chunkSizeMb;
        float timeElapsed = 30;

        long actualSize = 0;
        while (true)
        {
            if (stop)
            {
                return;
            }
            while (true)
            {
                if (stop)
                {
                    return;
                }
                chunkSizeMb = (int)(chunkSizeMb / timeElapsed * 30);
                if (chunkSizeMb > Program.settings.configuration.chunkSizeMb)
                {
                    chunkSizeMb = Program.settings.configuration.chunkSizeMb;
                }
                else if (chunkSizeMb < 1)
                {
                    chunkSizeMb = 1;
                }

                string outputFilePath = FilesSplitter.SplitFile(filePath, cacheDirectoryPath
                    , actualSize, chunkSizeMb * 1024 * 1024, chunkIndex);
                Stopwatch stopwatch = new Stopwatch();
                try
                {
                    stopwatch.Start();
                    await channel.SendFileAsync(outputFilePath, "!give " + '"' + Path.GetFileName(filePath).Replace(" ", "_") + '"');
                    stopwatch.Stop();
                    timeElapsed = stopwatch.ElapsedMilliseconds / 1000;
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("The sending of the file part failed, probably beceause the file part is too big for your internet. " +
                        "Don't worry, we try again with a smaller size");
                    timeElapsed = 100;
                    stopwatch.Stop();
                }
            }
            
            actualSize += chunkSizeMb * 1024 * 1024;

            if (((float)(actualSize) / (float)GetFileSize() * 100) >= 100)
            {
                await message.ModifyAsync(x => x.Content = $"100% Sent!");
            }
            else
            {
                await message.ModifyAsync(x => x.Content = $"{((float)(actualSize) / (float)GetFileSize() * 100):N2}% Sent...");
            }

            if (GetFileSize() <= actualSize)
            {
                break;
            }
            
            chunkIndex++;
        }
        
        while (true)
        {
            if (stop)
            {
                return;
            }
            await channel.SendMessageAsync("!Assemble " + '"' + Path.GetFileName(filePath).Replace(" ", "_") + '"' + " " + (chunkIndex + 1));
            await Task.Delay(20000);
        }
    }

    public async Task SendMissingFile()
    {
        var channel = Program.discordBot.client.GetChannel(Program.settings.configuration.channelId) as ISocketMessageChannel;

        for (int i = 0; i < missingFiles.Count; i++)
        {
            if (stop)
            {
                return;
            }
            if (File.Exists(Path.GetFileName(filePath) + "." + missingFiles[i]))
            {
                continue;
            }
            await channel.SendFileAsync(Path.Combine(Program.filesStorage.filePartsSendingPath, Path.GetFileName(filePath)
            , Path.GetFileName(filePath) + "." + missingFiles[i]), $"!Give {'"'}{Path.GetFileName(filePath)}{'"'}");
        }

        missingFiles = new List<string>();

        await channel.SendMessageAsync("!Assemble " + '"' + Path.GetFileName(filePath)
            .Replace(" ", "_") + '"' + " " + (chunkIndex + 1));
    }
    private long GetFileSize()
    {
        FileInfo fileInfo = new FileInfo(filePath);
        return fileInfo.Length;
    }

    public async void Cancel(SocketCommandContext context)
    {
        stop = true;
        await context.Channel.SendMessageAsync("Canceling...");
        var channel = Program.discordBot.client.GetChannel(Program.settings.configuration.channelId) as ISocketMessageChannel;
        await channel.SendMessageAsync($"!DeleteReceivedCache {Path.GetFileName(filePath)}");
        await context.Channel.SendMessageAsync($"{Path.GetFileName(filePath)} Canceled!");
    }
}
