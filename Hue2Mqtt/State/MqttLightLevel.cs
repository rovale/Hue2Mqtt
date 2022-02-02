using Hue2Mqtt.HueApi;

namespace Hue2Mqtt.State;

internal class MqttLightLevel : MqttDevice
{
    public MqttLightLevel(string topic, HueResource hueResource) : base(topic)
    {
        if (hueResource.LightLevel is { IsValid: true })
        {
            LightLevel = hueResource.LightLevel.Value;
        }
    }

    public int? LightLevel { get; }
}