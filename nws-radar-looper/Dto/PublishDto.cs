namespace com.tandell.nws_radar_looper.Dto;

public class PublishDto
{
    public string BasePath { get; set; } = "./";

    ///
    /// Publish the latest image retrieved with a consistent name
    ///
    public bool Latest { get; set; } = true;

    /// Publish an animated image and publish it with a consistent name
    public bool Animated { get; set; } = true;

    // How long to make the animated loop
    public int Length {get; set;} = 10;

    // How long to pause on each image
    public int Delay {get; set;} = 250;


}