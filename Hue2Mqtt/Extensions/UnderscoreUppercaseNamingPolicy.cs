using System.Text.Json;
using Humanizer;

namespace Hue2Mqtt.Extensions;

internal class UnderscoreUppercaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        return name.Underscore().ToUpper();
    }
}