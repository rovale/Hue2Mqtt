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

            if (hueResource.Type == "light")
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

            return null;
        }

        protected MqttDevice(string topic)
        {
            Topic = topic;
        }
        public string Topic { get; }
    }
}