namespace com.tandell.nws_radar_looper.DataAccess;

public class NwsHttpClient(IHttpClientFactory httpFactory, ILogger<NwsHttpClient> logger)
{
    /// <summary>
    /// Make a request to the NWS Rest API. Throws an HttpRequestException, if not successful.
    /// </summary>
    /// <param name="method">The HTTP method to use</param>
    /// <param name="path">The Rest Endpoint to use</param>
    /// <returns>The Response of the Rest API, needs to be Dispose() after use</returns>
    public async Task<HttpResponseMessage> Request(HttpMethod method, string path)
    {
        logger.LogDebug("Starting NWS Request for Method:[{Method}] and Resource:[{UrlPath}]", method, path);

        using var client = httpFactory.CreateClient("NWS");
        using HttpRequestMessage request = new(method, path);
        HttpResponseMessage response = await client.SendAsync(request);

        return response.EnsureSuccessStatusCode();
    }
}