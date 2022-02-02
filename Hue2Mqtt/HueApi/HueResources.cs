public class HueResources
{
    public object[] errors { get; set; }
    public HueResource[] data { get; set; }
}

public partial class HueResource
{
    //public string id { get; set; }
    //public string id_v1 { get; set; }
    //public Identify identify { get; set; }
    public Metadata? metadata { get; set; }
    //public Product_Data product_data { get; set; }
    public Service[] services { get; set; }
    //public string type { get; set; }
}

//public class Identify
//{
//}

public class Metadata
{
    //public string archetype { get; set; }
    public string? name { get; set; }

    public int? control_id { get; set; }
}

//public class Product_Data
//{
//    public bool certified { get; set; }
//    public string hardware_platform_type { get; set; }
//    public string manufacturer_name { get; set; }
//    public string model_id { get; set; }
//    public string product_archetype { get; set; }
//    public string product_name { get; set; }
//    public string software_version { get; set; }
//}

public class Service
{
    public string rid { get; set; }
    public string rtype { get; set; }
}