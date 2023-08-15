using System.Globalization;

public class FilesSplitter
{

    //valeurs en octets
    public static string SplitFile(string filePath, string outputPathDirectory, long beginChunk, int chunkSize, int chunkNumber)
    {
        if (!File.Exists(filePath))
        {
            // Gérer le cas où le fichier source n'existe pas.
            return null;
        }

        if (!Directory.Exists(outputPathDirectory))
        {
            // Gérer le cas où le dossier de sortie n'existe pas.
            return null;
        }

        using (FileStream inputFileStream = File.OpenRead(filePath))
        {
            if (beginChunk + chunkSize >= inputFileStream.Length)
            {
                chunkSize = (int) (inputFileStream.Length - beginChunk);
            }

            inputFileStream.Seek(beginChunk, SeekOrigin.Begin);

            byte[] buffer = new byte[chunkSize];
            int bytesRead = inputFileStream.Read(buffer, 0, chunkSize);

            if (bytesRead > 0)
            {
                string chunkFilePath = Path.Combine(outputPathDirectory, $"{Path.GetFileName(filePath).Replace(" ", "_")}.{chunkNumber:D5}");

                using (FileStream outputFileStream = File.Create(chunkFilePath))
                {
                    outputFileStream.Write(buffer, 0, bytesRead);
                }
            }
        }

        return Path.Combine(outputPathDirectory, $"{Path.GetFileName(filePath).Replace(" ", "_")}.{chunkNumber:D5}");
    }

    public static string ReassembleFile(string fileName, int chunkNumber)
    {
        if (!Directory.Exists(Path.Combine(Program.filesStorage.filePartsReceivedPath, fileName)))
        {
            return "Error there is no files with this name";
        }
        //string[] chunkFiles = Directory.GetFiles(Path.Combine(Program.filesStorage.filePartsReceivedPath, fileName)
        //    , $"{fileName}.*");
        string[] chunkFiles = Directory.GetFiles(Path.Combine(Program.filesStorage.filePartsReceivedPath, fileName)
        , $"{fileName}.*").Select(Path.GetFileName).ToArray();
        if (chunkFiles.Length != chunkNumber)
        {
            List<string> missingFiles = new List<string>();
            for (int i = 0; i < chunkNumber; i++)
            {
                if (!chunkFiles.Contains($"{fileName}.{i:D5}"))
                {
                    missingFiles.Add($"{i:D5}");
                }
            }

            return $"!Ask {'"'}{fileName}{'"'} " + string.Join(",", missingFiles);
        }
        else
        {
            string filePath = Path.Combine(Program.filesStorage.downloadsPath, fileName);
            File.Create(filePath).Close();
            using (FileStream fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                foreach (string chunkName in chunkFiles)
                {
                    string chunkFilePath = Path.Combine(Program.filesStorage.filePartsReceivedPath
                        , fileName, chunkName);
                    byte[] chunkData = File.ReadAllBytes(chunkFilePath);
                    fileStream.Write(chunkData, 0, chunkData.Length);
                }
            }

            Program.filesStorage.DeleteDirectory(Path.Combine(Program.filesStorage.filePartsReceivedPath, fileName));
            return $"!DeleteSendingCache {'"'}{fileName}{'"'}";
        }
    }
}