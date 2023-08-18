using System.Diagnostics;
using System.Globalization;

public class FilesStorage
{
    public string projectName;
    public string directoryPath;

    public string settingsFilePath;
    public string downloadsPath;
    public string filePartsReceivedPath;
    public string filePartsSendingPath;
    public FilesStorage(String _projectName)
    {
        projectName = _projectName;
        directoryPath = Path.Combine(Environment.GetFolderPath
            (Environment.SpecialFolder.ApplicationData), "RemiDv", projectName);
    }

    public void CreateAllDirectories()
    {
        CreateDirectory(directoryPath);

        settingsFilePath = Path.Combine(directoryPath, "settings.json");

        downloadsPath = Path.Combine(directoryPath, "downloads");
        CreateDirectory(downloadsPath);

        CreateDirectory(Path.Combine(directoryPath, "cache"));

        filePartsReceivedPath = Path.Combine(directoryPath, "cache", "FilePartsReceived");
        CreateDirectory(filePartsReceivedPath);
        filePartsSendingPath = Path.Combine(directoryPath, "cache", "FilePartsSending");
        CreateDirectory(filePartsSendingPath);
    }

    public bool FirstTimeOpened()
    {
        return !Directory.Exists(directoryPath);
    }

    public async Task DownloadFile(string fileURL, string outputFilePath)
    {
        if (GetDownloadsSize() > Program.settings.configuration.maxSizeMb && Program.settings.configuration.maxSizeMb > 0)
        {
            return;
        }
        using (var httpClient = new HttpClient())
        {
            using (var stream = await httpClient.GetStreamAsync(fileURL))
            {
                // Copie le contenu du fichier joint dans le fichier local
                using (var fileStream = new FileStream(outputFilePath, FileMode.Create))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }
    }

    public long GetDownloadsSize()
    {
        long downloadsSize = GetDirectorySize(Program.filesStorage.downloadsPath) 
            + GetDirectorySize(Program.filesStorage.filePartsReceivedPath);
        return downloadsSize;
    }

    public long GetDirectorySize(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            return 0;
        }
        DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
        return GetDirectorySize(directoryInfo);
    }

    public long GetDirectorySize(DirectoryInfo directory)
    {
        long totalSize = 0;

        FileInfo[] files = directory.GetFiles();
        foreach (FileInfo file in files)
        {
            totalSize += file.Length;
        }

        DirectoryInfo[] subDirectories = directory.GetDirectories();
        foreach (DirectoryInfo subDirectory in subDirectories)
        {
            totalSize += GetDirectorySize(subDirectory);
        }

        return totalSize / (1024 * 1024);
    }

    public void CreateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public void ClearCache()
    {
        ClearDirectory(filePartsSendingPath);
        ClearDirectory(filePartsReceivedPath);
    }

    public void DeleteDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }  
    }

    public void ClearDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        string[] files = Directory.GetFiles(directoryPath);
        string[] directories = Directory.GetDirectories(directoryPath);

        foreach (string file in files)
        {
            File.Delete(file);
        }

        foreach (string directory in directories)
        {
            ClearDirectory(directory); // Appel récursif pour vider les sous-répertoires
            Directory.Delete(directory);
        }
    }

    public bool OpenExplorer(string folderPath)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = folderPath,
                UseShellExecute = true
            };

            Process.Start(startInfo);

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}