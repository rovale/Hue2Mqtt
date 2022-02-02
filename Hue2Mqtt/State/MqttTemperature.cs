namespace Hue2Mqtt.State;

internal class MqttTemperature : MqttDevice
{
    public MqttTemperature(string topic, HueResource hueResource) : base(topic)
    {
        if (hueResource.temperature != null && hueResource.temperature.temperature_valid)
        {
            Temperature = hueResource.temperature.temperature;
        }
    }

    public float? Temperature { get; }
}