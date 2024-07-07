# National Weather Service Stadard Radar Region Retriever

The NWS runs several weather radars, as is their overall purvue, and one of the services they offer is an over the top 'enhanced' version. Which is great and all. Until you need just a quick glance at what's happening. They also offer a 'standard', or low-res, version that's a series of gif images that can be looped to get a quick feel of what the weather is doing.

The only issue is that these images are transient and only shows the current radar images. 

This program will continuously download the radar images and save them to a local directory. In order to not be a bad net citizen, this program honors the cache-control max age response to determine when it should attempt to get the next available image. Some random jitter is also added so we can avoid possible traffic retrieving the same image at the exact same time.

## Known Radar Stations

In the configuration, it requires a radar station to be provided. There's not a good listing of available stations so the following are provided for ease of use. Other stations can be interactively discovered at https://radar.weather.gov/ 

- `CONUS` - Entire continential United States aggregate image
  - https://radar.weather.gov/ridge/standard/CONUS_0.gif
  - https://radar.weather.gov/ridge/standard/CONUS_loop.gif
- `KIWA` - Arizona
- `KCBX` - Western Idaho
- `KSFX` - Eastern Idaho
- `KMAX` - Southern Oregon

## Configuration Item Descriptions

- `Settings:BasePath`: The location where to save the retrieved images
- `Settings:DatePattern`: To keep the filenames unique and sortable, the current timestamp is leveraged. This is the format applied to the date so it's compact and scriptable
- `Settings:NwsRestApiUrl`: The base URL for the National Weather Service REST API
- `Settings:Station`: The NWS Station Id for the radar images; e.g. KIWA, KCBX, etc

- `Storage`: Details of where to store the downloaded Radar images and such

- `Archive:BasePath`: The path where the archives will be saved, needs to be readable/writable by the program. The system will check that it exists and the permissions are correct.
- `Archive:FilePattern`: This is the format applied to the date so it's compact and scriptable
- `Archive:ArchiveDelay`: The number of hours a file needs to exist before it's available for archiving. e.g. If the value is set to 18, only files _older_ than 18 hours will be considered for archiving. This is based off of the last time the file was written.

## Future Enhancements

- Deal with duplicate radar images
- Deal with missing radar images
- Figure out how to generate AOT binaries - that way we just have a single executable instead of a directory
- The images should be saved in a directory structure of something like `<basepath>/<station>/timestamp.gif`
- create animated gifs from the prior set of images for displaying loops
  - https://www.nuget.org/packages/Aspose.Imaging/
  - https://github.com/dlemstra/Magick.NET/blob/main/docs/CombiningImages.md
  - https://github.com/dlemstra/Magick.NET/blob/main/docs/ResizeImage.md
  - https://github.com/dlemstra/Magick.NET/blob/main/docs/Readme.md
  - https://github.com/dlemstra/Magick.NET/blob/main/docs/CrossPlatform.md
  - https://github.com/dlemstra/Magick.NET
  - https://kb.aspose.com/imaging/net/how-to-create-gif-from-images-in-csharp/
- Create a systemd file to run this as a service on linux
  - keep this running if it dies.
  - https://devblogs.microsoft.com/dotnet/net-core-and-systemd/
- Per https://weather-gov.github.io/api/general-faqs, NWS wants an email in the User-Agent header. 
  - Create a settings section for "information". Don't run w/o the email.
  - Add the User-Agent header to all requests. 
- Save headers to json for later processing - zip file entries can have a huge comment. Take the json file and insert it as a comment to the zip file. Determine json format for the comment as well.

### Future future Enhancements

- Provide a list of stations instead of just a single station

## Cavets

This project does create and delete files. While care has been taken to prevent unintended damage, there are no guarantees. Take care with the settings and verify them before executing.

## References

- https://wildermuth.com/2022/05/04/using-background-services-in-asp-net-core/
- https://radar.weather.gov/region/conus/standard