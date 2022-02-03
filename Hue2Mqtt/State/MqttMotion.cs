using Hue2Mqtt.HueApi;

namespace Hue2Mqtt.State;

internal class MqttMotion : MqttDevice
{
    public MqttMotion(string topic, HueResource hueResource) : base(topic)
    {
        UpdateFrom(hueResource);
    }

    public sealed override void UpdateFrom(HueResource hueResource)
    {
        if (hueResource.Motion != null)
        {
            Motion = hueResource.Motion.IsValid ? hueResource.Motion.Value : null;
        }
    }

    public bool? Motion { get; set; }
}