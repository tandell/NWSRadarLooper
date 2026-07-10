using System.IO.Compression;
using com.tandell.nws_radar_looper.DataAccess;
using com.tandell.nws_radar_looper.Dto;
using System.Globalization;
using ImageMagick;

namespace com.tandell.nws_radar_looper;

public class PublishClient(
	PublishDto publishConfiguration, 
	SettingsDto settings, 
	FileHandler fileHandler, 
	ILogger<FileClient> logger)
{
    /// <summary>
    /// Create and configure the file system watcher to trigger when a file is written/changed.
    /// </summary>    
    public FileSystemWatcher CreateFileWatcher() {
        ValidateBasePath();
        FileSystemWatcher watcher = new FileSystemWatcher(settings.BasePath);

        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += OnChanged;
        watcher.Filter = "*.gif";
        watcher.IncludeSubdirectories = false;
        watcher.EnableRaisingEvents = false;

        return watcher;
    }

    // Trigger fires when a file is changed on the system.
    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }
        
        logger.LogInformation("Changed: {FullPath}", e.FullPath );
        
        // Latest Image
        string? latest = DetermineNewestFile();
        PublishLatest(latest);

        // Animate Gif Logic
        DateTime utcNow = DateTime.UtcNow;
        DateTime cutoff = utcNow.AddHours(-2);
        List<string> files = DetermineFilesDuringTimeSpan(cutoff, utcNow);
        string animatedOutfileName = GenerateAnimatedGif(files);
    }

    void PublishLatest(string? latestFile) {
        if( string.IsNullOrEmpty(latestFile) ) {
            return;
        }

        // Full path to the original file
        string fullFilePath = Path.Combine(settings.BasePath, latestFile);

        // Check if the source file exists
        if (File.Exists(fullFilePath))
        {
            try
            {
                // Copy the file to the output directory with a new name 'latest.gif'
                string destinationFilePath = Path.Combine(publishConfiguration.BasePath, "latest.gif");
                
                File.Copy(fullFilePath, destinationFilePath, true);
                
                logger.LogInformation($"File copied successfully: {destinationFilePath}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while copying the file.");
            }
        }
        else
        {
            logger.LogError($"The expected source file '{fullFilePath}' does not exist.");
        }
    }

    /// <summary>
    /// Retrieve the filename of the newest file, based on creation time.
    /// </summary>
    string? DetermineNewestFile() {
        var fileInfoList = Directory.GetFiles(settings.BasePath, "*.gif")
                                .Select(file => new FileInfo(file))
                                .OrderByDescending(info => info.CreationTime);

        if (fileInfoList.Any())
        {
            logger.LogInformation($"The newest file is determined to be: {fileInfoList.First().Name}");
            return fileInfoList.First().Name;
        }

        return null;
    }

    /// <summary>
    /// Locate the filenames that fall between the provided timestamps, by filename.
    /// </summary>
    List<string> DetermineFilesDuringTimeSpan(DateTime filesFrom, DateTime filesTo) {
        // Get GIF files whose names are timestamps (e.g. 20240912T123000Z.gif)
        List<string> gifFiles = Directory.GetFiles(settings.BasePath, "*.gif")
            .Select(file =>
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                // Split timestamp and index
                int dashIndex = fileName.IndexOf('-');
                string timestampPart = fileName;
                int index = int.MaxValue; // default if no index present
                if (dashIndex >= 0)
                {
                    timestampPart = fileName.Substring(0, dashIndex);
                    string indexPart = fileName[(dashIndex + 1)..];
                    int.TryParse(indexPart, out index);
                }

                // Parse timestamp (UTC)
                if (DateTime.TryParseExact(
                        timestampPart,
                        "yyyyMMddTHHmmZ",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out DateTime timestamp))
                {
                    return new
                    {
                        File = file,
                        Timestamp = timestamp,
                        Index = index
                    };
                }

                // Skip files that don’t parse
                return null;
            })
            // TODO: Need to also have a stop cutoff, not just a start cuftoff.
            .Where(x => x != null && x.Timestamp >= filesFrom)
            // Sort first by Timestamp ascending, then by Index ascending
            .OrderBy(x => x.Timestamp)
            .ThenBy(x => x.Index)
            .Select(x => x.File)
            .ToList();

        if (!gifFiles.Any())
        {
            logger.LogDebug("No GIF files found within the past 2 hours.");
            return new List<string>();
        }

        foreach(var file in gifFiles) {
            logger.LogDebug("Found: {File}", file);
        }
        return gifFiles;
    }

    /// <summary>
    /// Generate the animated looping gif of the weather radar.
    /// </summary>
    string GenerateAnimatedGif(IList<string> files) 
    {
        var outputFile = publishConfiguration.BasePath + "/animated.gif";
        using (var collection = new MagickImageCollection())
        {
            for (int i = 0; i < files.Count; i++)
            {
                var gif = files[i];
                var image = new MagickImage(gif);

                // Ensure same size
                if (collection.Count > 0)
                {
                    image.Resize(collection[0].Width, collection[0].Height);
                }

                // Default delay 250 ms = 25/100 s
                uint delay = 25;

                // Make first frame slower
                if (i == 0) {
                    delay = 100; // 1 second
                }

                // Or make last frame slower
                if (i == files.Count - 1) {
                    delay = 100; // 1 second
                }

                image.AnimationDelay = delay;
                image.GifDisposeMethod = GifDisposeMethod.Background;

                collection.Add(image);
            }

            collection[0].AnimationIterations = 0; // infinite loop
            collection.Optimize();

            collection.Write(outputFile);
        }

        logger.LogInformation("Animated GIF generated: {OutputFile}", outputFile);
        return outputFile;
    }


    /// <summary>
    /// Validates and confirms the base path exists, etc. Eventually will check for permissions and all the details.
    /// Suppose sanitizing the path would be a good thing too. e.g. trim, remove trailing /, etc.
    /// </summary>
    void ValidateBasePath() {
        string basepath = publishConfiguration.BasePath;

        // Check to see if the path exists and is a directory.
        if( !Directory.Exists(basepath) )
        {
            logger.LogInformation("Creating path [{path}] for FileClient", basepath);
            Directory.CreateDirectory(basepath);
        }
    }
}