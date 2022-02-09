using System.Text.Json;
using System.Text.Json.Serialization;
using Hue2Mqtt.Extensions;
using Hue2Mqtt.State;
using Humanizer;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Serilog;

namespace Hue2Mqtt;

internal class MqttClient
{
    private readonly IMqttClientOptions _mqttClientOptions;
    private readonly IMqttClient _mqttClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter(new UnderscoreUppercaseNamingPolicy())
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = new UnderscoreNamingPolicy()
    };

    public MqttClient(string server, int port)
    {
        Log.Debug($"MQTT broker {server}:{port}");

        _mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(server, port)
            .Build();

        var factory = new MqttFactory();

        _mqttClient = factory.CreateMqttClient();

        _mqttClient.UseDisconnectedHandler(async e =>
        {
            Log.Warning("Disconnected from MQTT broker, reason: " + e.Reason);
            await Task.Delay(TimeSpan.FromSeconds(5));
            try
            {
                Log.Information("Reconnecting");
                await _mqttClient.ConnectAsync(_mqttClientOptions);
                Log.Information("Connected");
            }
            catch
            {
                Log.Error("Failed to reconnect");
            }
        });
    }

    public string CreateMqttTopic(params string[] nameParts)
    {
        return string.Join("/", nameParts).Underscore();
    }

    public async Task Connect()
    {
        await _mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);
    }

    public async Task Publish(string bridgeName, MqttDevice mqttDevice)
    {
        var json = JsonSerializer.Serialize(mqttDevice, mqttDevice.GetType(), _jsonSerializerOptions);

        Log.Information($"- {mqttDevice.Topic} - {json}");

        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"hue/{bridgeName}/{mqttDevice.Topic}".Underscore())
            .WithPayload(json)
            .Build();

        await _mqttClient.PublishAsync(message);
    }
}