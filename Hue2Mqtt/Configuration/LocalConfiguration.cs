using Microsoft.Extensions.Configuration;

namespace Hue2Mqtt.Configuration
{
    internal static class LocalConfiguration
    {
        public static IConfigurationRoot BuildConfiguration(string basePath, IConfigurationBuilder? configBuilder = null)
        {
            return (configBuilder ?? new ConfigurationBuilder())
                .SetBasePath(basePath)
                .AddUserSecrets("Hue2Mqtt")
                .AddIniFile("Hue2Mqtt.ini", true, true)
                .AddEnvironmentVariables("Hue2Mqtt_")
                .Build();
        }
    }
}
