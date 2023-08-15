using Discord.Commands;
using Discord.WebSocket;
using System.Globalization;

public class Commands : ModuleBase<SocketCommandContext>
{

    [Command("help")]
    public async Task HelpAsync() 
    {
        if (Context.User.Id != Program.settings.configuration.botAdminId)
        {
            return;
        }
        await ReplyAsync("```   Debidy Help menu:\n" +
        "- !Help -> Help menu\n" +
        "- !ShareFile filePath -> Start sending a file\n" +
        "- !Downloads -> Open the downloads folder\n" +
        "- !Cancel -> Cancel the sending of a file```");
    }

    [Command("Give")]
    public async Task GiveAsync(string fileName)
    {
        if (Context.User.Id != Program.settings.configuration.otherBotId)
        {
            await ReplyAsync("Only the other bot can send files");
            return;
        }
        if (Context.Message.Attachments.Count == 1)
        {
            var attachment = Context.Message.Attachments.FirstOrDefault();
            if (attachment != null)
            {
                Program.filesStorage.CreateDirectory(Path.Combine(Program.filesStorage.filePartsReceivedPath, fileName));
                string outputFilePath = Path.Combine(Program.filesStorage.filePartsReceivedPath, fileName, attachment.Filename);

                long maxSizeMb = Program.settings.configuration.maxSizeMb;
                long usedSize = Program.filesStorage.GetDownloadsSize();
                if (maxSizeMb <= 0)
                {
                    await ReplyAsync($"unlimited storage. Storage used: {usedSize}");
                }
                else
                {
                    await ReplyAsync(
                        $"storage used: {(float)usedSize / maxSizeMb * 100:N1}%" +
                        $", There is {maxSizeMb - usedSize}Mb storage not used");
                    if (usedSize / maxSizeMb >= 1)
                    {
                        await ReplyAsync($"STOCKAGE FULL {Program.discordBot.client.GetUser(Program.settings.configuration.botAdminId).Mention}. PLEASE MOVE FILES FROM THE !DOWNLOADS FOLDER");
                    }
                }
                await Program.filesStorage.DownloadFile(attachment.Url, outputFilePath);
                await Context.Message.DeleteAsync();
            }
        }
        else
        {
            await ReplyAsync("There is not 1 attachement to this message!");
        }
    }
    

    [Command("ShareFile")]
    public async Task ShareFile(string filePath)
    {
        if (Context.User.Id != Program.settings.configuration.botAdminId)
        {
            return;
        }

        if (Program.fileSender != null)
        {
            await ReplyAsync("You are already sending a file!");
            return;
        }
        if (!File.Exists(filePath))
        {
            await ReplyAsync("This file doesn't exist!");
            return;
        }
        SocketTextChannel channel = Program.discordBot.client.GetChannel(Context.Channel.Id) as SocketTextChannel;
        await ReplyAsync("Starting... (Do not use the file that you are sharing while sending)");
        Program.fileSender = new FileSender(filePath, channel);
    }

    [Command("Assemble")]
    public async Task AssembleAsync(string fileName, int chunkNumber)
    {
        if (Context.User.Id != Program.settings.configuration.otherBotId)
        {
            ReplyAsync("You are not the other bot!");
            return;
        }

        string message = FilesSplitter.ReassembleFile(fileName, chunkNumber);
        await ReplyAsync(message);
    }

    [Command("Ask")]
    public async Task Ask(string fileName, string missingFilesNumber)
    {
        if (Context.User.Id != Program.settings.configuration.otherBotId)
        {
            await ReplyAsync("You are not the other bot!");
            return;
        }
        if (Program.fileSender != null)
        {
            string[] missingFiles = missingFilesNumber.Split(',');
            for (int i = 0; i < missingFiles.Length; i++)
            {
                Program.fileSender.missingFiles.Add(missingFiles[i]);
            }
            Program.fileSender.SendMissingFile();
        }
    }

    [Command("DeleteSendingCache")]
    public async Task DeleteSendingCacheAsync(string fileName)
    {
        if (Context.User.Id != Program.settings.configuration.otherBotId)
        {
            return;
        }
        string directoryPath = Path.Combine(Program.filesStorage.filePartsSendingPath, fileName);
        Program.filesStorage.DeleteDirectory(directoryPath);
        Program.fileSender = null;
    }

    [Command("Downloads")]
    public async Task OpenDownloadsFolderAsync()
    {
        if (Context.User.Id != Program.settings.configuration.botAdminId)
        {
            return;
        }
        await ReplyAsync("Trying to open...");
        if (!Program.filesStorage.OpenExplorer(Program.filesStorage.downloadsPath))
        {
            await ReplyAsync(@"Failed to open, you can find your downloads at 
                C:\Users\USER(modify to yours)\AppData\Roaming\RemiDv\Debidy\downloads");
        }
        else
        {
            await ReplyAsync("The folder is opened!");
        }
    }

    [Command("Cancel")]
    public async Task CancelAsync()
    {
        if (Context.User.Id != Program.settings.configuration.botAdminId)
        {
            return;
        }
        Program.fileSender.Cancel(Context);
        Program.fileSender = null;
    }

    [Command("DeleteReceivedCache")]
    public async Task DeleteReceivedCacheAsync(string fileName)
    {
        if (Context.User.Id != Program.settings.configuration.otherBotId)
        {
            return;
        }
        string directoryPath = Path.Combine(Program.filesStorage.filePartsReceivedPath, fileName);
        Program.filesStorage.DeleteDirectory(directoryPath);
    }
}