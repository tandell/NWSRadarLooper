using com.tandell.nws_radar_looper;

// TODO un-minify this

var builder = Host.CreateApplicationBuilder(args);

var services = builder.Services;

builder.Services.AddHostedService<Worker>();

services.AddSingleton<NwsClient>();

services.AddHttpClient("NWS", (serviceProvider, client) =>
{
    client.BaseAddress = new Uri("https://radar.weather.gov");
});

var host = builder.Build();
host.Run();
