namespace Hue2Mqtt.State
{
    internal abstract class MqttDevice
    {
        public static MqttDevice CreateFrom(string topic, HueResource hueResource)
        {
            if (hueResource.type == "button")
            {
                return new MqttButton(topic, hueResource);
            }

            if (hueResource.type == "device_power")
            {
                return new MqttDevicePower(topic, hueResource);
            }

            if (hueResource.type == "light")
            {
                return new MqttLight(topic, hueResource);
            }

            if (hueResource.type == "light_level")
            {
                return new MqttLightLevel(topic, hueResource);
            }

            if (hueResource.type == "motion")
            {
                return new MqttMotion(topic, hueResource);
            }

            if (hueResource.type == "temperature")
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