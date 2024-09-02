namespace com.tandell.nws_radar_looper.Workers;

/// <summary>
/// LoopWorker sets up and executes the file system watcher that creates the gifs from the 
/// downloaded images.
/// The FileSystemWatcher logic is a bit strange in that it appears it's a TSR-type process.
/// During the lifetime, it handles events automatically without having to poll the process.
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
        // TODO warning CS8602: Dereference of a possibly null reference.
        watcher.EnableRaisingEvents = false;
        await base.StopAsync(cancellationToken);
    }

    // NOOP that BackgroundService requires
    // TODO warning CS1998: This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
    }

}
