# National Weather Service Stadard Radar Region Retriever

The NWS runs several weather radars, as is their overall purvue, and one of the services they offer is an over the top 'enhanced' version. Which is great and all. Until you need just a quick glance at what's happening. They also offer a 'standard', or low-res, version that's a series of gif images that can be looped to get a quick feel of what the weather is doing.

The only issue is that these images are transient and only shows the current radar images. 

This program will continuously download the radar images and save them to a local directory. In order to not be a bad net citizen, this program honors the cache-control max age response to determine when it should attempt to get the next available image. Some random jitter is also added so we can avoid possible traffic retrieving the same image at the exact same time.

## Configuration Item Descriptions

- `BasePath`: The location where to save the retrieved images. 
- `DatePattern`: To keep the filenames unique and sortable, the current timestamp is leveraged. This is the format applied to the date so it's compact and scriptable.
- `NwsRestApiUrl`: The base URL for the National Weather Service REST API.

## Future Enhancements

- Configuration: currently everything is hardcoded. At the very least, the station should be customizable. e.g. kiwa for Arizona, etc.
- The images should be saved in a directory structure of something like `<basepath>/<station>/timestamp.gif`
- create animated gifs from the prior set of images for displaying loops
- create a systemd file to keep this running if it dies.