namespace com.tandell.nws_radar_looper.Workers;

/// <summary>
/// RetrieverWorker is the BackgroundService that retrieves the radar images from the NWS.
/// Currently does not support support multiple stations.
/// </summary>
public class CleanupWorker(FileClient fileClient, ILogger<CleanupWorker> logger) : BackgroundService
{
    // TODO: add configuration option for the defaultDelay. Need to bump this to hours instead of minutes.
    int defaultDelay = 300 * 1000; // TODO Extend to 24 hours.

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("File Cleanup Worker running at: {time}", DateTimeOffset.Now);
            fileClient.ArchiveFiles();
            logger.LogInformation("Next cleanup run scheduled in {Delay} seconds", Math.Ceiling(defaultDelay/1000.0));
            await Task.Delay(defaultDelay, stoppingToken);
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken) 
    {
        logger.LogDebug("Starting File Cleanup Worker");
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Stopping File Cleanup Worker");
        await base.StopAsync(cancellationToken);
    }
}
