namespace Hue2Mqtt.State;

internal class MqttLight : MqttDevice
{
    public MqttLight(string topic, HueResource hueResource) : base(topic)
    {
        State = hueResource.on!.on ? PowerState.On : PowerState.Off;

        if (hueResource.dimming != null)
        {
            Brightness = hueResource.dimming.brightness;
        }

        if (hueResource.color != null)
        {
            Color = new Color
            {
                X = hueResource.color.xy.x,
                Y = hueResource.color.xy.y
            };
        }

        if (hueResource.color_temperature != null && hueResource.color_temperature.mirek_valid)
        {
            ColorTemperature = hueResource.color_temperature.mirek;
        }
    }

    public PowerState State { get; }
    public float? Brightness { get; }
    public int? ColorTemperature { get; }
    public Color? Color { get; }
}