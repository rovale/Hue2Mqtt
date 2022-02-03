using System.Text.Json.Serialization;
using Hue2Mqtt.HueApi;

namespace Hue2Mqtt.State
{
    internal abstract class MqttDevice
    {
        public static MqttDevice CreateFrom(string topic, HueResource hueResource)
        {
            if (hueResource.Type == "button")
            {
                return new MqttButton(topic, hueResource);
            }

            if (hueResource.Type == "device_power")
            {
                return new MqttDevicePower(topic, hueResource);
            }

            if (hueResource.Type == "light" || hueResource.Type == "grouped_light")
            {
                return new MqttLight(topic, hueResource);
            }

            if (hueResource.Type == "light_level")
            {
                return new MqttLightLevel(topic, hueResource);
            }

            if (hueResource.Type == "motion")
            {
                return new MqttMotion(topic, hueResource);
            }

            if (hueResource.Type == "temperature")
            {
                return new MqttTemperature(topic, hueResource);
            }

            throw new NotSupportedException($"Unknown device type {hueResource.Type}");
        }

        protected MqttDevice(string topic)
        {
            Topic = topic;
        }

        [JsonIgnore]
        public string Topic { get; }

        public abstract void UpdateFrom(HueResource hueResource);
    }
}