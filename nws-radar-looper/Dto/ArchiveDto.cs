namespace com.tandell.nws_radar_looper.Dto;

public class ArchiveDto
{
    public string BasePath { get; set; } = "./";
    public string FilePattern { get; set; } = "yyyyMMdd";
    public int ArchiveDelay {get; set;} = 24;
}
