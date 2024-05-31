namespace com.tandell.nws_radar_looper.Dto;

public class SettingsDto
{
    public string BasePath { get; set; } = "./";
    public string Pattern { get; set; } = "yyyyMMddTHHmmK";
    public string NwsRestApiUrl { get; set; } = "https://radar.weather.gov";
    public string Station {get; set; } = "KCBX";
}