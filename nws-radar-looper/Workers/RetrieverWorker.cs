using com.tandell.nws_radar_looper.Dto;

namespace com.tandell.nws_radar_looper.Workers;

/// <summary>
/// RetrieverWorker is the BackgroundService that retrieves the radar images from the NWS.
/// Currently does not support support multiple stations.
/// </summary>
public class RetrieverWorker(NwsClient client, ILogger<RetrieverWorker> logger) : BackgroundService
{
    private HeaderDto responseDto = new HeaderDto();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Should this be an instance variable?
        Random random = new Random();
        int delay;
        int sequentialFailures = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            try
            {
                responseDto = await client.GetImage(responseDto);
                delay = (responseDto.CacheControl + random.Next(5, 20)) * 1000;
                sequentialFailures = 0;
            }
            catch( Exception e ) 
            {
                // Should probably just do Polly for retrying
                logger.LogError(e, "Error executing GetImage!");
                delay = 20*1000;
                if( sequentialFailures > 10 ) 
                {
                    logger.LogError("More than 10 sequential failures occurred, exiting");
                    break;
                }
                sequentialFailures++;
            }
            
            logger.LogInformation("Next run scheduled in {Delay} seconds", Math.Ceiling(delay/1000.0));
            await Task.Delay(delay, stoppingToken);
        }
    }

    /// <summary>
    /// On start logic for the RetrieverWorker. Preload existing radar images.
    /// </summary>
    public override async Task StartAsync(CancellationToken cancellationToken) 
    {
        logger.LogDebug("Starting NWS Stadard Radar Region Retriever");

        // At this point, retrieve images 0..9 to "preload".
        // Note that the headers of the _0 call should be saved from the loop, that way there isn't a
        // duplicate image on the first call of the service.
        logger.LogDebug("Preloading current radar images...");
        for(int i = 9; i >= 0; i--)
        {
          logger.LogDebug("> Retrieving Radar Image {Image}", i);
          responseDto = await client.GetImage(i);
        }
        logger.LogDebug("Preloading Complete");

        await base.StartAsync(cancellationToken);
    }

    // Not sure if this is needed.... eval and remove later.
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Stopping NWS Stadard Radar Region Retriever");
        await base.StopAsync(cancellationToken);
    }
}
