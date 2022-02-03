using Hue2Mqtt.HueApi;

namespace Hue2Mqtt.State;

internal class MqttTemperature : MqttDevice
{
    public MqttTemperature(string topic, HueResource hueResource) : base(topic)
    {
        UpdateFrom(hueResource);
    }

    public sealed override void UpdateFrom(HueResource hueResource)
    {
        if (hueResource.Temperature != null)
        {
            Temperature = hueResource.Temperature.IsValid ? hueResource.Temperature.Value : null;
        }
    }

    public float? Temperature { get; set; }
}