namespace Hue2Mqtt.State;

internal class MqttMotion : MqttDevice
{
    public MqttMotion(string topic, HueResource hueResource) : base(topic)
    {
        if (hueResource.motion is { motion_valid: true })
        {
            Motion = hueResource.motion.motion;
        }
    }

    public bool? Motion { get; }
}