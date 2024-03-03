using com.tandell.nws_radar_looper;
using com.tandell.nws_radar_looper.DataAccess;
using com.tandell.nws_radar_looper.Dto;
using com.tandell.nws_radar_looper.Workers;

// TODO un-minify this

var builder = Host.CreateApplicationBuilder(args);

var services = builder.Services;

SettingsDto settings = builder.Configuration.GetRequiredSection("Settings").Get<SettingsDto>();

builder.Services.AddHostedService<RetrieverWorker>();

services.AddHttpClient("NWS", (serviceProvider, client) =>
{
    client.BaseAddress = new Uri(settings.NwsRestApiUrl);
});

services.AddSingleton<NwsHttpClient>();
services.AddSingleton<NwsClient>();
services.AddSingleton<SettingsDto>(settings);


var host = builder.Build();
host.Run();
