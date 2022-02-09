using System.Text.Json;
using Humanizer;

namespace Hue2Mqtt.Extensions;

internal class UnderscoreNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        return name.Underscore();
    }
}