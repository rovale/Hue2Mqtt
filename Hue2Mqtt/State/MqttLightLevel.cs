using Hue2Mqtt.HueApi;

namespace Hue2Mqtt.State;

internal class MqttLightLevel : MqttDevice
{
    public MqttLightLevel(string topic, HueResource hueResource) : base(topic)
    {
        UpdateFrom(hueResource);
    }

    public sealed override void UpdateFrom(HueResource hueResource)
    {
        if (hueResource.LightLevel != null)
        {
            LightLevel = hueResource.LightLevel.IsValid ? hueResource.LightLevel.Value : null;
        }
    }

    public int? LightLevel { get; set; }
}