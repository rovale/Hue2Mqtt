namespace Hue2Mqtt.Configuration
{
    public class ApplicationOptions
    {
        public string HueBaseAddress { get; set; }
        public string HueKey { get; set; }
        public string MqttServer { get; set; }
        public int MqttPort { get; set; }
    }
}
