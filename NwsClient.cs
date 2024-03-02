using com.tandell.nws_radar_looper.DataAccess;
using com.tandell.nws_radar_looper.Dto;

namespace com.tandell.nws_radar_looper;

public class NwsClient(NwsHttpClient nwsHttpClient, ILogger<NwsClient> logger)
{
    public async Task<HeaderDto> GetImage(HeaderDto? priorHeaders = null)
    {
        string expectedEtag = string.Empty;

        // If the priorHeaders is present, execute a HEAD request and only proceed if the ETags don't match.
        if (priorHeaders != null)
        {
            var headHeader = await GetCurrentImageHead("KCBX_0.gif");
            logger.LogDebug("Current Image Header: {Header}", headHeader.ToString());
            if (headHeader.ETag == priorHeaders.ETag)
            {
                return headHeader;
            }

            expectedEtag = headHeader.ETag;
        }

        var currentHeader = await GetCurrentImage("KCBX_0.gif");
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

        }

        // No matter if we had to retrieve prior versions, return the current header.
        return currentHeader;
    }

    private async Task<HeaderDto> GetCurrentImage(string imageName)
    {
        HttpResponseMessage response = await nwsHttpClient.Request(HttpMethod.Get, "ridge/standard/" + imageName);

        var responseHeaders = HeaderDto.ToHeaderDto(response);

        var filename = responseHeaders.FileName();
        var tempfile = filename + ".gif";
        logger.LogInformation("Saving image to {Filename}", tempfile);

        int opt = 0;
        while (Path.Exists(tempfile))
        {
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
    private async Task<HeaderDto> GetCurrentImageHead(string imageName)
    {
        HttpResponseMessage response = await nwsHttpClient.Request(HttpMethod.Head, "ridge/standard/" + imageName);

        var responseHeaders = HeaderDto.ToHeaderDto(response);

        response.Dispose();
        return responseHeaders;
    }
}