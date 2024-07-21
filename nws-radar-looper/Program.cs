using System.Net.Http.Headers;

using com.tandell.nws_radar_looper;
using com.tandell.nws_radar_looper.DataAccess;
using com.tandell.nws_radar_looper.Dto;
using com.tandell.nws_radar_looper.Workers;

// TODO un-minify this

var builder = Host.CreateApplicationBuilder(args);

var services = builder.Services;

SettingsDto? settingsConfiguration = builder.Configuration.GetRequiredSection("Settings")?.Get<SettingsDto>() ?? null;
if( settingsConfiguration == null ) 
{
    //TODO figure out what to do in case of invalid configuration
    throw new Exception("Invalid configuration, check README.md");
}
ArchiveDto? archiveConfiguration = builder.Configuration.GetRequiredSection("Archive")?.Get<ArchiveDto>() ?? null;
if( archiveConfiguration == null ) 
{
    //TODO figure out what to do in case of invalid configuration
    throw new Exception("Invalid archive configuration, check README.md");
}
ConfigurationDto? configurationConfiguration = builder.Configuration.GetRequiredSection("Configuration")?.Get<ConfigurationDto>() ?? null;
if( configurationConfiguration == null )
{
    //TODO figure out what to do in case of invalid configuration
    throw new Exception("Invalid configuration configuration, check README.md");
}

// Register the service to download the images
builder.Services.AddHostedService<RetrieverWorker>();
// Register the service to archive and remove the old images
builder.Services.AddHostedService<CleanupWorker>();
// Register the service to build the animated images
builder.Services.AddHostedService<LoopWorker>();

// Create a named HttpClient with the requested headers
services.AddHttpClient("NWS", (serviceProvider, client) =>
{
    client.BaseAddress = new Uri(settingsConfiguration.NwsRestApiUrl);

    // Set the User Agent for the application; including requested email address for contact
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("nws-radar-looper", "0.5"));
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue($"(+{configurationConfiguration.Email};{configurationConfiguration.Fork})"));
});
services.AddSingleton<NwsHttpClient>();

// Hosted Worker Clients
services.AddSingleton<NwsClient>();
services.AddSingleton<FileClient>();

services.AddSingleton<FileHandler>();

// Save the configuration for DI use; of course.
services.AddSingleton<ConfigurationDto>(configurationConfiguration);
services.AddSingleton<SettingsDto>(settingsConfiguration);
services.AddSingleton<ArchiveDto>(archiveConfiguration);

var host = builder.Build();
host.Run();