namespace Hue2Mqtt.State;

internal class MqttLightLevel : MqttDevice
{
    public MqttLightLevel(string topic, HueResource hueResource) : base(topic)
    {
        if (hueResource.light is { light_level_valid: true })
        {
            LightLevel = hueResource.light.light_level;
        }
    }

    public int? LightLevel { get; }
}