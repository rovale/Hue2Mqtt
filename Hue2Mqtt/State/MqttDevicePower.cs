namespace Hue2Mqtt.State;

internal class MqttDevicePower : MqttDevice
{
    public MqttDevicePower(string topic, HueResource hueResource) : base(topic)
    {
        if (hueResource.power_state != null)
        {
            BatteryLevel = hueResource.power_state.battery_level;
            BatteryState = hueResource.power_state.battery_state;
        }
    }

    public int BatteryLevel { get; }
    public string BatteryState { get; }
}