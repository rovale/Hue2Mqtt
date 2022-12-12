using Hue2Mqtt.HueApi;

namespace Hue2Mqtt.State;

internal class MqttLight : MqttDevice
{
    public MqttLight(string topic, HueResource hueResource) : base(topic)
    {
        UpdateFrom(hueResource);
    }

    public sealed override void UpdateFrom(HueResource hueResource)
    {
        if (hueResource.OnOffState != null)
        {
            State = hueResource.OnOffState.IsOn ? PowerState.On : PowerState.Off;
        }

        if (hueResource.Brightness != null)
        {
            Brightness = hueResource.Brightness.Value;
        }

        if (hueResource.Color is { Value: { } })
        {
            Color = new Color
                {
                    X = hueResource.Color.Value.X,
                    Y = hueResource.Color.Value.Y
                };
        }

        if (hueResource.ColorTemperature != null)
        {
            ColorTemperature = hueResource.ColorTemperature.IsValid ? hueResource.ColorTemperature.Value : null;
        }
    }

    public PowerState? State { get; private set; }
    public float? Brightness { get; private set; }
    public int? ColorTemperature { get; private set; }
    public Color? Color { get; private set; }
}