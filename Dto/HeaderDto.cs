using System.Net.Http.Headers;

namespace com.tandell.nws_radar_looper.Dto;

public class HeaderDto {
    public string ETag {get; set;} = "";
    public DateTimeOffset? Date {get; set;}
    public DateTimeOffset? LastModified {get; set;}
    public Uri? Url {get; set;}
    public int CacheControl {get; set;} = 0;
    public long ContentLength {get; set;} = -1;
    public string ContentType {get; set;} = "";

    public string FileName( ) {
        // TODO: The DateTime.UtcNow should be defaulted when the HeaderDto is initialized...
        DateTimeOffset date = LastModified ?? Date ?? DateTime.UtcNow;

        // TODO: Pull pattern from configuration or default.
        string pattern = "yyyyMMddTHHmmK";
        
        var filename = date.UtcDateTime.ToUniversalTime().ToString(pattern);

        return filename;
    }

    public override string ToString()
    {
        return $"{Url} - {Date} - {ETag} - {CacheControl} - {ContentType} - {ContentLength} - {FileName()}";
    }

    /// <summary>
    /// Create the HeaderDto from the provided HttpResponseMessage.
    /// </summary>
    /// <param name="response">The HttpResponseMessage to interrogate for information</param>
    /// <returns>The HeaderDto that contains the information in the response</returns>
    public static HeaderDto ToHeaderDto( HttpResponseMessage response) {
        if (response is null)
        {
            return new HeaderDto();
        }

        var headers = response.Headers;
        
        return new HeaderDto{
            Url = response.RequestMessage?.RequestUri ?? null,
            ETag = headers.ETag?.Tag ?? "",
            Date = headers.Date?.UtcDateTime,
            CacheControl = ParseMaxAge(headers.CacheControl),
            ContentLength = response.Content?.Headers?.ContentLength ?? -1,
            ContentType = response.Content?.Headers?.ContentType?.ToString() ?? "", 
            LastModified = response.Content?.Headers?.LastModified
        };
    }

    /// <summary>
    /// Because the MaxAge of the CacheControlHeaderValue is a struct, you can't use the fancy C#
    /// nullable handlers... I think.
    /// </summary>
    /// <param name="cachecontrol">The Cache-Control header to pull the max-age from</param>
    /// <returns>The number of seconds the Cache-Control max-age is set for; or default</returns>
    private static int ParseMaxAge( CacheControlHeaderValue? cachecontrol ) {
        int defaultValue = 60;

        var maxAge = cachecontrol?.MaxAge;
        if( !maxAge.HasValue ) {
            return defaultValue;
        }

        var totalSeconds = (int)maxAge.Value.TotalSeconds;

        if( totalSeconds < 1 ) {
            return defaultValue;
        } else {
            return totalSeconds;
        }
    }
}
