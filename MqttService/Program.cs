using MQTTnet.AspNetCore;
using MqttService.Controle;

var builder = WebApplication.CreateBuilder(args);


builder.WebHost.UseKestrel(o =>
{
    // This will allow MQTT connections based on TCP port 1883.
    o.ListenAnyIP(1883, l => l.UseMqtt());
    o.ListenAnyIP(5000); 
});

builder.Services.AddControllers();
builder.Services.AddHostedMqttServer(
    optionsBuilder =>
    {
        optionsBuilder.WithDefaultEndpoint();
    });

builder.Services.AddMqttConnectionHandler();
builder.Services.AddConnections();

builder.Services.AddSingleton<MqttController>();



var app = builder.Build();

var mqttController = app.Services.GetService<MqttController>();

app.MapGet("/", () => "Hello World!");

app.UseRouting();

app.UseEndpoints(
    endpoints =>
    {
        endpoints.MapConnectionHandler<MqttConnectionHandler>(
            "/mqtt",
            httpConnectionDispatcherOptions => httpConnectionDispatcherOptions.WebSockets.SubProtocolSelector =
                protocolList => protocolList.FirstOrDefault() ?? string.Empty);
    });

app.UseMqttServer(
    server =>
    {
        /*
         * Attach event handlers etc. if required.
         */

        server.ValidatingConnectionAsync += mqttController.ValidateConnection;
        server.ClientConnectedAsync += mqttController.OnClientConnected;
    });

app.Run();