using com.tandell.nws_radar_looper.DataAccess;
using com.tandell.nws_radar_looper.Dto;

namespace com.tandell.nws_radar_looper;

public class NwsClient(NwsHttpClient nwsHttpClient, SettingsDto settings, FileHandler fileHandler, ILogger<NwsClient> logger)
{
    private static string API_PATH = "ridge/standard/";

    /// <summary>
    /// Retrieve the current simple radar image; accounting for duplicates and such.
    /// </summary>
    /// <param name="priorHeaders">The headers for the prior image, or null</param>
    /// <returns>The headers of the retrieved image</returns>
    public async Task<HeaderDto> GetImage(HeaderDto? priorHeaders = null)
    {
        // Image name is the station followed by _<image number>.gif; where 0 is the most recent image.
        string imageName = $"{settings.Station}_0.gif";

        string expectedEtag = string.Empty;

        // If the priorHeaders is present, execute a HEAD request and only proceed if the ETags don't match.
        if (priorHeaders != null)
        {
            var headHeader = await GetCurrentImageHead(imageName);
            logger.LogDebug("Current Image Header: {Header}", headHeader.ToString());
            if (headHeader.ETag == priorHeaders.ETag)
            {
                return headHeader;
            }

            expectedEtag = headHeader.ETag;
        }

        var currentHeader = await GetCurrentImage(imageName);
        logger.LogDebug("New Current Header: {Header}", currentHeader.ToString());

        // At this point, check to see if the ETags between the current HEAD and the priorHeaders
        // matches (or priorHeaders weren't provided). If they don't match, that means we missed an
        // image somehow. Good news is that the images are still there, just with different names.
        // E.g. The current radar image will be KCBX_0, the current-1 image will be KCBX_1, 
        // current-2 will be KCBX-2, etc. At a certain point, the prior images do stop but for
        // retrieving the immediately missed one, it works.
        //
        // I _think_ we can compare the direct files, but the -1 file will be different than -0 
        // version.
        if (expectedEtag != string.Empty && expectedEtag != currentHeader.ETag)
        {
            logger.LogError($"IMAGE MISSED. {expectedEtag}:{currentHeader.ETag}");
            // TODO Retrieve previous
        }

        // No matter if we had to retrieve prior versions, return the current header.
        return currentHeader;
    }

    /// <summary>
    /// Retrieve the requested image name from the NWS. 
    /// </summary>
    /// <param name="imageName">The image name to request from NWS</param>
    /// <returns>The headers of the call</returns>
    private async Task<HeaderDto> GetCurrentImage(string imageName)
    {
        HttpResponseMessage response = await nwsHttpClient.Request(HttpMethod.Get, API_PATH + imageName);

        var responseHeaders = HeaderDto.ToHeaderDto(response);

        var tempfile = fileHandler.GenerateFilename(responseHeaders);

        // TODO: Move logic to fileHandler class.
        // TODO: After saving, if we had a file name collision, md5sum the two images. 
        // If they're the same, just remove the extra files.
        using (FileStream fs = new FileStream(tempfile.Filename, FileMode.CreateNew))
        using (Stream input = response.Content.ReadAsStream())
        {
            input.CopyTo(fs);
        }

        response.Dispose();

        if( tempfile.ExistedPrior ) {
            // TODO Handle duplicates/invalid naming.
            logger.LogInformation("Duplicate filename detected");
        }
        
        return responseHeaders;
    }

    /// <summary>
    /// Retrieve the headers for the requested image name from the NWS. This allows us to check for
    /// a new image before requesting the image.
    /// </summary>
    /// <param name="imageName">The image name to request from NWS</param>
    /// <returns>The headers of the call</returns>
    private async Task<HeaderDto> GetCurrentImageHead(string imageName)
    {
        HttpResponseMessage response = await nwsHttpClient.Request(HttpMethod.Head, API_PATH + imageName);

        var responseHeaders = HeaderDto.ToHeaderDto(response);

        response.Dispose();
        return responseHeaders;
    }
}