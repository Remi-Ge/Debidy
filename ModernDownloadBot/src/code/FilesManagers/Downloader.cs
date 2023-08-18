using Discord.Commands;

public class Downloader
{
    public List<pendingCommand> pendingCommands = new List<pendingCommand>();

    public Downloader()
    {
        Task.Run(DownloadTask);
    }

    public void AddCommand(string _fileName, string _fileUrl, int _chunkNumber, SocketCommandContext _context)
    {
        for (int i = 0; i < pendingCommands.Count; i++) 
        {
            if (pendingCommands[i] == new pendingCommand(_fileName, _fileUrl, _chunkNumber, _context))
            {
                return;
            }
        }
        pendingCommands.Add(new pendingCommand(_fileName, _fileUrl, _chunkNumber, _context));
        //if (!pendingCommands.Contains(new pendingCommand(_fileName, _fileUrl, _chunkNumber, _context)))
        //{
        //    pendingCommands.Add(new pendingCommand(_fileName, _fileUrl, _chunkNumber, _context));
        //}
    }
    public async Task DownloadTask()
    {
        while (true)
        {
            if (pendingCommands.Count > 0)
            {
                await pendingCommands[0].executeCommand();
                pendingCommands.RemoveAt(0);
            }
            else
            {
                await Task.Delay(5000);
            }
        }
    }

    public class pendingCommand
    {
        public string fileName;
        public string fileUrl;
        public int chunkNumber;
        private SocketCommandContext context;
        
        public pendingCommand(string _fileName, string _fileUrl, int _chunkNumber, SocketCommandContext _context)
        {
            fileName = _fileName;
            fileUrl = _fileUrl;
            chunkNumber = _chunkNumber;
            context = _context;
        }
        
        public static bool operator ==(pendingCommand cmd1, pendingCommand cmd2)
        {
            if (cmd1.fileName == cmd2.fileName && cmd1.fileUrl == cmd2.fileUrl && cmd1.chunkNumber == cmd2.chunkNumber)
            {
                return true;
            }

            return false;
        }
        
        public static bool operator !=(pendingCommand cmd1, pendingCommand cmd2)
        {
            if (cmd1.fileName == cmd2.fileName && cmd1.fileUrl == cmd2.fileUrl && cmd1.chunkNumber == cmd2.chunkNumber)
            {
                return false;
            }

            return true;
        }

        public async Task executeCommand()
        {
            if (fileUrl != null)
            {
                string outputFilePath = Path.Combine(Program.filesStorage.filePartsReceivedPath, fileName, context.Message.Attachments.FirstOrDefault().Filename);

                long maxSizeMb = Program.settings.configuration.maxSizeMb;
                long usedSize = Program.filesStorage.GetDownloadsSize();
                if (maxSizeMb <= 0)
                {
                    await context.Channel.SendMessageAsync($"unlimited storage. Storage used: {usedSize}");
                }
                else
                {
                    await context.Channel.SendMessageAsync(
                        $"storage used: {(float)usedSize / maxSizeMb * 100:N1}%" +
                        $", There is {maxSizeMb - usedSize}Mb storage not used");
                    if (usedSize / maxSizeMb >= 1)
                    {
                        await context.Channel.SendMessageAsync($"STOCKAGE FULL {Program.discordBot.client.GetUser(Program.settings.configuration.botAdminId).Mention}. PLEASE MOVE FILES FROM THE !DOWNLOADS FOLDER");
                    }
                }
                await Program.filesStorage.DownloadFile(fileUrl, outputFilePath);
                await context.Message.DeleteAsync();
            }
            else if (chunkNumber > 0)
            {
                string message = FilesSplitter.ReassembleFile(fileName, chunkNumber);
                await context.Channel.SendMessageAsync(message);
            }
            else
            {
                string directoryPath = Path.Combine(Program.filesStorage.filePartsReceivedPath, fileName);
                Program.filesStorage.DeleteDirectory(directoryPath);
            }
        }
    }
}