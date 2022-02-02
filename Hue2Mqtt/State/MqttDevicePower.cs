using Hue2Mqtt.HueApi;

namespace Hue2Mqtt.State;

internal class MqttDevicePower : MqttDevice
{
    public MqttDevicePower(string topic, HueResource hueResource) : base(topic)
    {
        if (hueResource.Battery != null)
        {
            BatteryLevel = hueResource.Battery.Level;
            BatteryState = hueResource.Battery.State;
        }
    }

    public int BatteryLevel { get; }
    public string BatteryState { get; }
}