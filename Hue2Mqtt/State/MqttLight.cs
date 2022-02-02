using Hue2Mqtt.HueApi;

namespace Hue2Mqtt.State;

internal class MqttLight : MqttDevice
{
    public MqttLight(string topic, HueResource hueResource) : base(topic)
    {
        State = hueResource.OnOffState!.IsOn ? PowerState.On : PowerState.Off;

        if (hueResource.Brightness != null)
        {
            Brightness = hueResource.Brightness.Value;
        }

        if (hueResource.Color != null)
        {
            Color = new Color
            {
                X = hueResource.Color.Value.X,
                Y = hueResource.Color.Value.Y
            };
        }

        if (hueResource.ColorTemperature is { IsValid: true })
        {
            ColorTemperature = hueResource.ColorTemperature.Value;
        }
    }

    public PowerState State { get; }
    public float? Brightness { get; }
    public int? ColorTemperature { get; }
    public Color? Color { get; }
}