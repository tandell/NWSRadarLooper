using com.tandell.nws_radar_looper;
using com.tandell.nws_radar_looper.DataAccess;
using com.tandell.nws_radar_looper.Dto;
using com.tandell.nws_radar_looper.Workers;

// TODO un-minify this

var builder = Host.CreateApplicationBuilder(args);

var services = builder.Services;

SettingsDto? settings = builder.Configuration.GetRequiredSection("Settings")?.Get<SettingsDto>() ?? null;
if( settings == null ) 
{
    //TODO figure out what to do in case of invalid configuration
    throw new Exception("Invalid configuration, check README.md");
}

builder.Services.AddHostedService<RetrieverWorker>();

services.AddHttpClient("NWS", (serviceProvider, client) =>
{
    client.BaseAddress = new Uri(settings.NwsRestApiUrl);
});

services.AddSingleton<NwsHttpClient>();
services.AddSingleton<NwsClient>();
services.AddSingleton<FileHandler>();
services.AddSingleton<SettingsDto>(settings);


var host = builder.Build();
host.Run();
