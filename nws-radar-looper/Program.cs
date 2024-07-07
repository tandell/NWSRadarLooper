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


builder.Services.AddHostedService<RetrieverWorker>();
builder.Services.AddHostedService<CleanupWorker>();
builder.Services.AddHostedService<LoopWorker>();

services.AddHttpClient("NWS", (serviceProvider, client) =>
{
    client.BaseAddress = new Uri(settingsConfiguration.NwsRestApiUrl);
});

services.AddSingleton<NwsHttpClient>();

// Hosted Worker Clients
services.AddSingleton<NwsClient>();
services.AddSingleton<FileClient>();

services.AddSingleton<FileHandler>();
services.AddSingleton<SettingsDto>(settingsConfiguration);
services.AddSingleton<ArchiveDto>(archiveConfiguration);


var host = builder.Build();
host.Run();
