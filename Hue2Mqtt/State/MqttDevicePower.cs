using Hue2Mqtt.HueApi;

namespace Hue2Mqtt.State;

internal class MqttDevicePower : MqttDevice
{
    public MqttDevicePower(string topic, HueResource hueResource) : base(topic)
    {
        UpdateFrom(hueResource);
    }

    public sealed override void UpdateFrom(HueResource hueResource)
    {
        if (hueResource.Battery != null)
        {
            BatteryLevel = hueResource.Battery.Level;
            BatteryState = hueResource.Battery.State;
        }
    }

    public int? BatteryLevel { get; set; }
    public string? BatteryState { get; set; }
}