namespace com.tandell.nws_radar_looper;

public class NwsClient(IHttpClientFactory httpFactory, ILogger<NwsClient> logger)
{
    public async Task<HeaderDto> GetImage(HeaderDto priorHeaders) {
        var currentHeader = await GetCurrentImageHead("KCBX_0.gif");
        logger.LogDebug("Current Image Header: {Header}", currentHeader.ToString());

        if( currentHeader.ETag != priorHeaders.ETag) {
            currentHeader = await GetCurrentImage("KCBX_0.gif");
            // TODO
            // At this point, we can compare the ImageHead.Etag with the Image.ETag. If they don't
            // match, this indicates that we missed an image. Retrieve KCBX_1 with a filename of 
            // KCBX_0 - 1 minute, then continue as usual. Something something race condition.
            // however, this _shouldn't_ happen...
            logger.LogDebug("New Current Header: {Header}", currentHeader.ToString());
        } 

        // By returning the 'currentHeader' everytime, we get an updated CacheControl value for the
        // delay even if we didn't get a new image.
        return currentHeader;
    }

    public async Task<HeaderDto> GetCurrentImage( string imageName ) {
        using var client = httpFactory.CreateClient("NWS");
        using HttpRequestMessage request = new(HttpMethod.Get, "ridge/standard/" + imageName );
        using HttpResponseMessage response = await client.SendAsync(request);

        var responseHeaders = HeaderDto.ToHeaderDto(response);

        var filename = responseHeaders.FileName();
        var tempfile = filename + ".gif";
        logger.LogInformation("Saving image to {Filename}", tempfile);

        int opt = 0;
        while( Path.Exists(tempfile) ) {
            logger.LogError("File already exists! [{Filename}] Last-Modified: [{LastModified}] Date: [{Date}]", tempfile, responseHeaders.LastModified, responseHeaders.Date);
            tempfile = filename + "-" + opt + ".gif";
        }

        using (FileStream fs = new FileStream(tempfile, FileMode.CreateNew))
        using (Stream input = response.Content.ReadAsStream())
        {
            input.CopyTo(fs);
        }

        return responseHeaders;
    }

    public async Task<HeaderDto> GetCurrentImageHead( string imageName ) {
        using var client = httpFactory.CreateClient("NWS");
        using HttpRequestMessage request = new(HttpMethod.Head, "ridge/standard/" + imageName );
        using HttpResponseMessage response = await client.SendAsync(request);

        var responseHeaders = HeaderDto.ToHeaderDto(response);

        return responseHeaders;
    }
}