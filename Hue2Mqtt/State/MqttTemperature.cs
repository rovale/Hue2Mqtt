using Hue2Mqtt.HueApi;

namespace Hue2Mqtt.State;

internal class MqttTemperature : MqttDevice
{
    public MqttTemperature(string topic, HueResource hueResource) : base(topic)
    {
        if (hueResource.Temperature is { IsValid: true })
        {
            Temperature = hueResource.Temperature.Value;
        }
    }

    public float? Temperature { get; }
}