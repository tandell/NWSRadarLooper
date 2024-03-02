using com.tandell.nws_radar_looper.DataAccess;
using com.tandell.nws_radar_looper.Dto;

namespace com.tandell.nws_radar_looper;

public class NwsClient(NwsHttpClient nwsHttpClient, ILogger<NwsClient> logger)
{
    public async Task<HeaderDto> GetImage(HeaderDto? priorHeaders = null) {
        // If the priorHeaders is present, execute a HEAD request and only proceed if the ETags don't match.
        if( priorHeaders != null ) 
        {
            var headHeader = await GetCurrentImageHead("KCBX_0.gif");
            logger.LogDebug("Current Image Header: {Header}", headHeader.ToString());
            if( headHeader.ETag == priorHeaders.ETag) {
                return headHeader;
            }
        }

        // At this point, either the ETags between the current HEAD and the priorHeaders doesn't
        // match _or_ priorHeaders weren't provided.
        var currentHeader = await GetCurrentImage("KCBX_0.gif");
        logger.LogDebug("New Current Header: {Header}", currentHeader.ToString());

        // TODO
        // At this point, we can compare the ImageHead.Etag with the Image.ETag. If they don't
        // match, this indicates that we missed an image. Retrieve KCBX_1 with a filename of 
        // KCBX_0 - 1 minute, then continue as usual. Something something race condition.
        // however, this _shouldn't_ happen...
        //
        // Need to see what the last-modified header for KCBX_1 is compared to KCBX_0

        return currentHeader;
    }

    public async Task<HeaderDto> GetCurrentImage( string imageName ) {
        HttpResponseMessage response = await nwsHttpClient.Request(HttpMethod.Get, "ridge/standard/" + imageName);

        var responseHeaders = HeaderDto.ToHeaderDto(response);

        var filename = responseHeaders.FileName();
        var tempfile = filename + ".gif";
        logger.LogInformation("Saving image to {Filename}", tempfile);

        int opt = 0;
        while( Path.Exists(tempfile) ) {
            //File already exists! [20240302T1843Z.gif] Last-Modified: [03/02/2024 18:43:17 +00:00] Date: [03/02/2024 18:52:32 +00:00]
            logger.LogError("File already exists! [{Filename}] Last-Modified: [{LastModified}] Date: [{Date}]", tempfile, responseHeaders.LastModified, responseHeaders.Date);
            tempfile = filename + "-" + opt + ".gif";
            opt++;
        }

        // TODO: After saving, if we had a file name collision, md5sum the two images. If they're the same, just remove the extra files.

        using (FileStream fs = new FileStream(tempfile, FileMode.CreateNew))
        using (Stream input = response.Content.ReadAsStream())
        {
            input.CopyTo(fs);
        }

        response.Dispose();
        return responseHeaders;
    }

    /// <summary>
    /// Retrieve the headers for the requested image name from the NWS. This allows us to check for
    /// a new image before requesting the image.
    /// </summary>
    /// <param name="imageName">The image name to request from NWS</param>
    /// <returns>The headers of the call</returns>
    public async Task<HeaderDto> GetCurrentImageHead( string imageName ) {
        HttpResponseMessage response = await nwsHttpClient.Request(HttpMethod.Head, "ridge/standard/" + imageName);

        var responseHeaders = HeaderDto.ToHeaderDto(response);

        response.Dispose();
        return responseHeaders;
    }
}