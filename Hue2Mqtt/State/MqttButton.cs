using Hue2Mqtt.HueApi;

namespace Hue2Mqtt.State;

internal class MqttButton : MqttDevice
{
    public MqttButton(string topic, HueResource hueResource) : base(topic)
    {
        if (hueResource.Button != null)
        {
            Event = hueResource.Button.LastEvent;
        }
    }

    public string Event { get; }
}