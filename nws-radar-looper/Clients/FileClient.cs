using System.IO.Compression;
using com.tandell.nws_radar_looper.Dto;

namespace com.tandell.nws_radar_looper;

public class FileClient(SettingsDto settings, ILogger<FileClient> logger){

    /// <summary>
    /// ArchiveFiles will discover any images written longer than 24 hours ago. It will then take
    //  those files, determine the correct archive file to use. It will then compress the images
    // for long term storage.
    /// </summary>
    public void ArchiveFiles() {
        string basepath = settings.BasePath;
        var currentTimeUtc = DateTime.UtcNow;

        var directory = new DirectoryInfo(basepath);
        // TODO: Convert the hours into a settings option.
        var files = directory.EnumerateFiles()
            .Where(f => f.LastWriteTimeUtc < currentTimeUtc.AddHours(-24))
            .OrderByDescending(f => f.LastWriteTime);
        logger.LogInformation(">>> Current Time: {Time}", currentTimeUtc);
        
        Dictionary<string, ZipArchive> archives = new Dictionary<string, ZipArchive>();

        try {
            foreach( var file in files ) 
            {
                // Determine the archive filename and path.
                var filedate = ConvertTimestampToPattern(file.LastWriteTimeUtc);
                // TODO: Add in path configuration from settings.
                var archiveFileName = $"{filedate}.zip";

                logger.LogInformation("Using archive with name [{name}.zip]", filedate);

                // Determine if the archive file is already open. If it's not, open it and cache
                // the object for later use.
                // TODO: Figure out "Converting null literal or possible null value to non-nullable type." on the out param.
                if(!archives.TryGetValue(filedate, out ZipArchive archive) ) 
                {
                    logger.LogDebug("Opening new archive with name [{name}]", archiveFileName);
                    // TODO once the path information is determined above, no need for zipPath.
                    string zipPath = $"/tmp/{archiveFileName}";
                    archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);
                    archives.Add(filedate, archive);
                }
                
                if( !archive.Entries.Any(entry => entry.Name == file.Name) )
                { 
                    logger.LogInformation("Adding file [{filename}] to archive", file.Name);
                    archive.CreateEntryFromFile(file.FullName, file.Name);
                } 
                else
                {
                    logger.LogInformation("File NOT added due to file already existing in archive");
                }
            }
        } finally {
            // Close every open archive; since we're not "using" them, it needs to be done.
            archives.ToList().ForEach(pair => {
                logger.LogInformation("Closing [{filename}.zip]", pair.Key);
                pair.Value.Dispose();
            });
        }
    }

    string ConvertTimestampToPattern(DateTimeOffset dateTime)
    {
        // TODO: Whoops, need to customize the pattern. Just need yyyyMMdd.zip
        return dateTime.UtcDateTime.ToUniversalTime().ToString(settings.Pattern);
    }
}