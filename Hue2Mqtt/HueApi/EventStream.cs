public class EventStream
{
    public DateTime creationtime { get; set; }
    public HueResource[] data { get; set; }
    public string id { get; set; }
    public string type { get; set; }
}

public partial class HueResource
{
    public string id { get; set; }
    public string type { get; set; }
    public On? on { get; set; }
    public Dimming? dimming { get; set; }
    public Color? color { get; set; }
    public Color_Temperature? color_temperature { get; set; }
    public Temperature? temperature { get; set; }
    public Motion? motion { get; set; }
    public LightLevel? light { get; set; }
    public PowerState? power_state { get; set; }
    public Button? button { get; set; }
}

public class On
{
    public bool on { get; set; }
}

public class Dimming
{
    public float brightness { get; set; }
}

public class Color
{
    public Xy xy { get; set; }
}

public class Xy
{
    public float x { get; set; }
    public float y { get; set; }
}

public class Color_Temperature
{
    public int? mirek { get; set; }
    public bool mirek_valid { get; set; }
}

public class Temperature
{
    public float? temperature { get; set; }
    public bool temperature_valid { get; set; }
}

public class Motion
{
    public bool? motion { get; set; }
    public bool motion_valid { get; set; }
}

public class LightLevel
{
    public int? light_level { get; set; }
    public bool light_level_valid { get; set; }
}

public class PowerState
{
    public int battery_level { get; set; }
    public string battery_state { get; set; }
}

public class Button
{
    public string last_event { get; set; }
}
