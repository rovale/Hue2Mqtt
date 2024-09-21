using System.Text.Json.Serialization;
using Hue2Mqtt.HueApi;

namespace Hue2Mqtt.State
{
    internal class MqttDevice
    {
        internal MqttDevice(string topic)
        {
            Topic = topic;
        }

        [JsonIgnore]
        public string Topic { get; }

        public string? Event { get; set; }
        public int? BatteryLevel { get; set; }
        public string? BatteryState { get; set; }
        public PowerState? State { get; private set; }
        public float? Brightness { get; private set; }
        public int? ColorTemperature { get; private set; }
        public Color? Color { get; private set; }
        public int? LightLevel { get; set; }
        public bool? Motion { get; set; }
        public float? Temperature { get; set; }
        public string? Status { get; set; }
        public bool? Online { get; set; }

        public void UpdateFrom(HueResource hueResource)
        {
            if (hueResource.Button != null)
            {
                Event = hueResource.Button.LastEvent?.ToUpper();
            }

            if (hueResource.Battery != null)
            {
                BatteryLevel = hueResource.Battery.Level;
                BatteryState = hueResource.Battery.State;
            }

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

            if (hueResource.LightLevel != null)
            {
                LightLevel = hueResource.LightLevel.IsValid ? hueResource.LightLevel.Value : null;
            }

            if (hueResource.Motion != null)
            {
                Motion = hueResource.Motion.IsValid ? hueResource.Motion.Value : null;
            }

            if (hueResource.Temperature != null)
            {
                Temperature = hueResource.Temperature.IsValid ? hueResource.Temperature.Value : null;
            }

            if (hueResource.Status != null)
            {
                Online = null;
                Status = null;

                if (hueResource.Status == "connected")
                {
                    Online = true;
                }
                else if (hueResource.Status == "connectivity_issue")
                {
                    Online = false;
                }
                else
                {
                    Status = hueResource.Status;
                }
            }
        }
    }
}