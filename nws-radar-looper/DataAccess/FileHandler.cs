using System.IO;
using System.Text;
using System.Security.Cryptography;

using com.tandell.nws_radar_looper.Dto;

namespace com.tandell.nws_radar_looper.DataAccess;

/// <summary>
/// Handles all file system access for a particular image
/// </summary>
/// <param name="logger"></param>
public class FileHandler(SettingsDto settings, ILogger<FileHandler> logger)
{
    /// <summary>
    /// Compute the MD5 sum of the provided file. Caution, not for use with medium and larger size
    /// files.
    /// No error handling, so make sure the file exists and is readable before use.
    /// </summary>
    /// <param name="filename">Fully qualified filename with path</param>
    /// <returns>The MD5 Hash of the provided filename</returns>
    public string ComputeFileHash(string filename)
    {
        using (var md5 = MD5.Create())
        {
            var byteHash = md5.ComputeHash(File.ReadAllBytes(filename));
            return BitConverter.ToString(byteHash).Replace("-","");
        }
    }

    /// <summary>
    /// Generate the absolute filename for the image based on it's headers. If the filename 
    /// collides with an existing file, append an integer to the filename.
    /// </summary>
    /// <param name="headers">Headers to retrieve the information from</param>
    /// <returns>FileDto configuration object; including the unique filename</returns>
    public FileDto GenerateFilename(HeaderDto headers)
    {
        var filename = DetermineFileDateName(headers);

        var fileDto = new FileDto {
            BaseFilename = filename,
            Filename = settings.BasePath + filename + "-0.gif"
        };

        logger.LogInformation("Saving image to {Filename}", fileDto.Filename);

        while (Path.Exists(fileDto.Filename))
        {
            fileDto.Iteration++;
            fileDto.Filename = settings.BasePath + filename + "-" + fileDto.Iteration + ".gif";
            fileDto.ExistedPrior = true;

            //File already exists! [20240302T1843Z.gif] Last-Modified: [03/02/2024 18:43:17 +00:00] Date: [03/02/2024 18:52:32 +00:00]
            logger.LogError("File already exists! [{Filename}] Last-Modified: [{LastModified}] Date: [{Date}]", fileDto.Filename, headers.LastModified, headers.Date);
        }

        return fileDto;
    }

    /// <summary>
    /// Generate the base filename from the Date, after applying the formatting pattern from
    /// configuration.
    /// </summary>
    /// <param name="headers">Headers to retrieve the Date information from</param>
    /// <returns>The formatted base filename for later use</returns>
    private string DetermineFileDateName(HeaderDto headers)
    {
        DateTimeOffset date = headers.LastModified ?? headers.Date ?? DateTime.UtcNow;

        return date.UtcDateTime.ToUniversalTime().ToString(settings.Pattern);
    }
}

public class FileDto 
{
    /// <summary>
    /// The path of the file
    /// </summary>
    public string Filename {get; set;} = string.Empty;

    /// <summary>
    /// ExistedPrior is a flag to represent that the filename had a collision and that data
    /// validation needs to occur.
    /// </summary>
    public bool ExistedPrior {get; set;} = false;

    /// <summary>
    /// BaseFilename of the file; without extension, iteration, or base path
    /// </summary>
    public string BaseFilename {get; set;} = string.Empty;

    /// <summary>
    /// The 'iteration' of the file. Anything greater than 0 means a duplicate
    /// </summary>
    public int Iteration {get; set;} = 1;
}