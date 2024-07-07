using System.IO.Compression;
using com.tandell.nws_radar_looper.DataAccess;
using com.tandell.nws_radar_looper.Dto;

namespace com.tandell.nws_radar_looper;

public class FileClient(ArchiveDto archiveConfiguration, SettingsDto settings, ILogger<FileClient> logger)
{

    /// <summary>
    /// Create and configure the file system watcher to trigger when a file is  written/changed.
    /// </summary>    
    public FileSystemWatcher CreateFileWatcher() {
        // TODO: Validate BasePath & create if missing

        FileSystemWatcher watcher = new FileSystemWatcher(settings.BasePath);

        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += OnChanged;
        watcher.Filter = "*.gif";
        watcher.IncludeSubdirectories = false;
        watcher.EnableRaisingEvents = false;

        return watcher;
    }

    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        // TODO: There _should_ be a way to pass in a logger instance. Looks like the 
        // `watcher.Changed += OnChanged` seems to be the 'simple' way of registering events.
        // https://learn.microsoft.com/en-us/dotnet/api/system.io.filesystemeventhandler?view=net-8.0
        // https://learn.microsoft.com/en-us/dotnet/standard/events/
        // https://learn.microsoft.com/en-us/dotnet/api/system.io.renamedeventhandler?view=net-8.0
        // https://stackoverflow.com/questions/10863471/how-do-i-pass-a-variable-to-an-event-in-c
        // https://stackoverflow.com/questions/55712573/access-method-from-filesystemwatcher-event-handler-in-c-sharp
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }
        //logger.LogInformation("Changed: {FullPath}", e.FullPath);
        Console.WriteLine($"Changed: {e.FullPath}");
    }

    /// <summary>
    /// ArchiveFiles will discover any images written longer than 24 hours ago. It will then take
    /// those files, determine the correct archive file to use. It will then compress the images
    /// for long term storage.
    /// </summary>
    public void ArchiveFiles() 
    {
        ValidateBasePath();

        string basepath = settings.BasePath;
        var currentTimeUtc = DateTime.UtcNow;

        var directory = new DirectoryInfo(basepath);
        var files = directory.EnumerateFiles()
            .Where(f => f.LastWriteTimeUtc < currentTimeUtc.AddHours(GetConfiguredAgeThreshold()) 
                && f.Extension == ".gif")
            .OrderByDescending(f => f.LastWriteTime);
        
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
                if(!archives.TryGetValue(filedate, out ZipArchive? archive) ) 
                {
                    logger.LogDebug("Opening new archive with name [{name}]", archiveFileName);
                    // TODO once the path information is determined above, no need for zipPath.
                    string zipPath = $"{archiveConfiguration.BasePath}/{archiveFileName}";
                    archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);
                    archives.Add(filedate, archive);
                }
                
                // At this point, we're just doing a filename match and shoving files into various 
                // zipfiles before deleting the file. At the same point, we're saving the MD5 sum
                // as a comment on the entry. The thought is there might be a filename collision 
                // during some error case, such as a network blip, thus while the filenames are the
                // same, the data isn't. And the MD5 sum will show the issue. And allow for one
                // final chance at saving the radar image before removing it as a duplicate.

                if( !archive.Entries.Any(entry => entry.Name == file.Name) )
                { 
                    var filehash = FileHandler.ComputeFileHash(file.FullName);
                    logger.LogInformation("Adding file [{filename}][{filehash}] to archive", file.Name, filehash);
                    var entry = archive.CreateEntryFromFile(file.FullName, file.Name);
                    entry.Comment = filehash;
                }
                
                try {
                    logger.LogInformation("Removing file [{filename}] from system", file.Name);
                    file.Delete();
                } catch( Exception e) {
                    logger.LogError(e, "Issue removing file {filename}", file.Name);
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

    // Suspect this should live in the DTO....
    int GetConfiguredAgeThreshold() 
    {
        int delay = archiveConfiguration.ArchiveDelay;

        if( delay > 0 )
        {
            delay = delay * -1; // negate 
        } else if (delay == 0) 
        {
            delay = -24; // default for invalid configuration.
        }

        return delay;
    }

    string ConvertTimestampToPattern(DateTimeOffset dateTime)
    {
        var pattern = archiveConfiguration.FilePattern;
        return dateTime.UtcDateTime.ToUniversalTime().ToString(pattern);
    }

    /// <summary>
    /// Validates and confirms the base path exists, etc. Eventually will check for permissions and all the details.
    // Suppose sanitizing the path would be a good thing too. e.g. trim, remove trailing /, etc.
    /// </summary>
    void ValidateBasePath() {
        string basepath = archiveConfiguration.BasePath;

        // Check to see if the path exists and is a directory.
        if( !Directory.Exists(basepath) )
        {
            throw new Exception("Archive:BasePath Invalid");
        }
    }
}