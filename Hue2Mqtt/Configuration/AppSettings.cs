using System.ComponentModel.DataAnnotations;

namespace Hue2Mqtt.Configuration
{
    public class AppSettings
    {
        [Required]
        public string HueBaseAddress { get; set; }

        [Required]
        public string HueKey { get; set; }

        [Required] 
        public string MqttServer { get; set; }

        [Required, Range(1, 65535)] 
        public int MqttPort { get; set; }
    }
}
