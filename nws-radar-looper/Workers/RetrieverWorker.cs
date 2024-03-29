using com.tandell.nws_radar_looper.Dto;

namespace com.tandell.nws_radar_looper.Workers;

/// <summary>
/// RetrieverWorker is the BackgroundService that retrieves the radar images from the NWS.
/// Currently does not support support multiple stations.
/// </summary>
public class RetrieverWorker(NwsClient client, ILogger<RetrieverWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Random random = new Random();

        HeaderDto responseDto = new HeaderDto();
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            responseDto = await client.GetImage(responseDto);

            int delay = (responseDto.CacheControl + random.Next(5, 20)) * 1000;
            await Task.Delay(delay, stoppingToken);
        }
    }
}
