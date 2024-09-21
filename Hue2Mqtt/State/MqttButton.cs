using Hue2Mqtt.HueApi;

namespace Hue2Mqtt.State;

internal class MqttButton : MqttDevice
{
    public MqttButton(string topic, HueResource hueResource) : base(topic)
    {
        UpdateFrom(hueResource);
    }

    public sealed override void UpdateFrom(HueResource hueResource)
    {
        Event = hueResource.Button?.LastEvent?.ToUpper();
    }

    public string? Event { get; set; }
}