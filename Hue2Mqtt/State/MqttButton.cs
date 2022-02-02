namespace Hue2Mqtt.State;

internal class MqttButton : MqttDevice
{
    public MqttButton(string topic, HueResource hueResource) : base(topic)
    {
        if (hueResource.button != null)
        {
            Event = hueResource.button.last_event;
        }
    }

    public string Event { get; }
}