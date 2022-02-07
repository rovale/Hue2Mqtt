using System.Text.Json;
using System.Text.Json.Serialization;
using Hue2Mqtt.State;
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
            new JsonStringEnumConverter()
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public MqttClient()
    {
        _mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("raspberrypi", 1883)
            .Build();

        var factory = new MqttFactory();

        _mqttClient = factory.CreateMqttClient();

        _mqttClient.UseDisconnectedHandler(async e =>
        {
            Log.Warning("### DISCONNECTED FROM SERVER ### Reason: " + e.Reason);
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await _mqttClient.ConnectAsync(_mqttClientOptions);
            }
            catch
            {
                Log.Error("### RECONNECTING FAILED ###");
            }
        });
    }

    public async Task Connect()
    {
        await _mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic("Rovale/Hue2Mqtt")
            .WithPayload("Hello World")
            .Build();

        await _mqttClient.PublishAsync(message);
    }

    public async Task Publish(MqttDevice mqttDevice)
    {
        var json = JsonSerializer.Serialize(mqttDevice, mqttDevice.GetType(), _jsonSerializerOptions);

        Log.Information($"- {mqttDevice.Topic} - {json}");

        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"Rovale/Hue2Mqtt/{mqttDevice.Topic}")
            .WithPayload(json)
            .Build();

        await _mqttClient.PublishAsync(message);
    }
}