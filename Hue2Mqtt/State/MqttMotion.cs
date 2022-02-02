using Hue2Mqtt.HueApi;

namespace Hue2Mqtt.State;

internal class MqttMotion : MqttDevice
{
    public MqttMotion(string topic, HueResource hueResource) : base(topic)
    {
        if (hueResource.Motion is { IsValid: true })
        {
            Motion = hueResource.Motion.Value;
        }
    }

    public bool? Motion { get; }
}