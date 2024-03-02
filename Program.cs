using com.tandell.nws_radar_looper;
using com.tandell.nws_radar_looper.DataAccess;
using com.tandell.nws_radar_looper.Workers;

// TODO un-minify this

var builder = Host.CreateApplicationBuilder(args);

var services = builder.Services;

builder.Services.AddHostedService<RetrieverWorker>();

services.AddHttpClient("NWS", (serviceProvider, client) =>
{
    client.BaseAddress = new Uri("https://radar.weather.gov");
});

services.AddSingleton<NwsHttpClient>();
services.AddSingleton<NwsClient>();


var host = builder.Build();
host.Run();
