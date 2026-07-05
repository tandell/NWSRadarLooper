# Tasks

- Validate that the `User-Agent` logic works as expected.

## Future Enhancements

- Deal with duplicate radar images
- Deal with missing radar images
- Figure out how to generate AOT binaries - that way we just have a single executable instead of a directory
- The images should be saved in a directory structure of something like `<basepath>/<station>/timestamp.gif` so that multiple instance can be run concurrently without issue.
- Create animated gifs from the prior set of images for displaying loops
  - https://www.nuget.org/packages/Aspose.Imaging/
  - https://github.com/dlemstra/Magick.NET/blob/main/docs/CombiningImages.md
  - https://github.com/dlemstra/Magick.NET/blob/main/docs/ResizeImage.md
  - https://github.com/dlemstra/Magick.NET/blob/main/docs/Readme.md
  - https://github.com/dlemstra/Magick.NET/blob/main/docs/CrossPlatform.md
  - https://github.com/dlemstra/Magick.NET
  - https://kb.aspose.com/imaging/net/how-to-create-gif-from-images-in-csharp/
  - https://learn.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/background-tasks-with-ihostedservice
- Create a systemd file to run this as a service on linux
  - keep this running if it dies.
  - https://devblogs.microsoft.com/dotnet/net-core-and-systemd/
- Validate that the email for the User-Agent has been provided. Nothing fancy at first, just that there's a value.
- Save headers to json for later processing - zip file entries can have a huge comment. Take the json file and insert it as a comment to the zip file. Determine json format for the comment as well.
- ~When a new image is retrieved, it should a) be copied to a "current" image; b) trigger the animated gif logic so that a new loop is generated.~
- On start, verify requested directories are writable.
  - Exit cleanly with an understandable error message if the directory isn't writable

### Future Future Enhancements

- Provide a list of stations instead of just a single station so that a single instance can be leveraged.
