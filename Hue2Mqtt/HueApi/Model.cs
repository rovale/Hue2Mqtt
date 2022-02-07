using System.Text.Json.Serialization;

namespace Hue2Mqtt.HueApi;

public class HueResources
{
    [JsonPropertyName("data")]
    public HueResource[] Data { get; set; }
}

public class Events
{
    [JsonPropertyName("creationtime")]
    public DateTime CreationTime { get; set; }
    
    [JsonPropertyName("data")]
    public HueResource[] Data { get; set; }
}

public class HueResource
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("metadata")]
    public Metadata? Metadata { get; set; }

    [JsonPropertyName("services")]
    public Service[] Services { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("on")]
    public OnOffState? OnOffState { get; set; }

    [JsonPropertyName("dimming")]
    public Brightness? Brightness { get; set; }

    [JsonPropertyName("color")]
    public Color? Color { get; set; }

    [JsonPropertyName("color_temperature")]
    public ColorTemperature? ColorTemperature { get; set; }

    [JsonPropertyName("temperature")]
    public Temperature? Temperature { get; set; }

    [JsonPropertyName("motion")]
    public Motion? Motion { get; set; }

    [JsonPropertyName("light")]
    public LightLevel? LightLevel { get; set; }

    [JsonPropertyName("power_state")]
    public Battery? Battery { get; set; }
    
    [JsonPropertyName("button")]
    public Button? Button { get; set; }
}

public class Metadata
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("control_id")]
    public int? ControlId { get; set; }
}

public class Service
{
    [JsonPropertyName("rid")]
    public string RelatedId { get; set; }

    [JsonPropertyName("rtype")]
    public string RelatedType { get; set; }
}


public class OnOffState
{
    [JsonPropertyName("on")]
    public bool IsOn { get; set; }
}

public class Brightness
{
    [JsonPropertyName("brightness")]
    public float Value { get; set; }
}

public class Color
{
    [JsonPropertyName("xy")]
    public Xy Value { get; set; }
}

public class Xy
{
    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }
}

public class ColorTemperature
{
    [JsonPropertyName("mirek")]
    public int? Value { get; set; }

    [JsonPropertyName("mirek_valid")]
    public bool IsValid { get; set; }
}

public class Temperature
{
    [JsonPropertyName("temperature")]
    public float? Value { get; set; }

    [JsonPropertyName("temperature_valid")]
    public bool IsValid { get; set; }
}

public class Motion
{
    [JsonPropertyName("motion")]
    public bool? Value { get; set; }

    [JsonPropertyName("motion_valid")]
    public bool IsValid { get; set; }
}

public class LightLevel
{
    [JsonPropertyName("light_level")]
    public int? Value { get; set; }

    [JsonPropertyName("light_level_valid")]
    public bool IsValid { get; set; }
}

public class Battery
{
    [JsonPropertyName("battery_level")]
    public int Level { get; set; }

    [JsonPropertyName("battery_state")]
    public string State { get; set; }
}

public class Button
{
    [JsonPropertyName("last_event")]
    public string LastEvent { get; set; }
}