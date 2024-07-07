namespace com.tandell.nws_radar_looper.Workers;

/// <summary>
/// RetrieverWorker is the BackgroundService that retrieves the radar images from the NWS.
/// Currently does not support support multiple stations.
/// </summary>
public class LoopWorker(FileClient fileClient, ILogger<LoopWorker> logger) : BackgroundService
{
    FileSystemWatcher? watcher = null;

    public override async Task StartAsync(CancellationToken cancellationToken) 
    {
        logger.LogInformation("Starting Loop Creator Worker");
        watcher = fileClient.CreateFileWatcher();

        // Enable the watcher after setup, etc.
        watcher.EnableRaisingEvents = true;
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Loop Creator Worker");
        await base.StopAsync(cancellationToken);
    }

    // NOOP that BackgroundService requires
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
    }

}
