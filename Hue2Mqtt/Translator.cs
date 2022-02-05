using System.Text.Json;
using System.Text.Json.Serialization;
using Hue2Mqtt.HueApi;
using Hue2Mqtt.State;
using Humanizer;

namespace Hue2Mqtt;

internal class Translator
{
    private readonly HueClient _hueClient;
    readonly Dictionary<string, MqttDevice> _mqttDevicesById = new();

    public Translator(HueClient hueClient)
    {
        _hueClient = hueClient;
    }

    public async Task Start()
    {
        await RegisterDevices();
        while (true)
        {
            try
            {
                await ProcessEventStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Retrying in 5 seconds");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }

    async Task RegisterDevices()
    {
        Console.WriteLine("Registering devices");
        var ignoredTypes = new[] { "bridge", "zigbee_connectivity", "entertainment" };

        var devices = await _hueClient.GetResources("device");

        foreach (var device in devices)
        {
            var deviceName = device.Metadata?.Name ?? "Unknown device";

            foreach (var relatedService in device.Services.Where(s => !ignoredTypes.Contains(s.RelatedType)))
            {
                var relatedServiceType = relatedService.RelatedType;
                var relatedServiceId = relatedService.RelatedId;

                var service = await _hueClient.GetResource(relatedServiceType, relatedServiceId);
                RegisterDevice(service, CreateMqttTopic(service, deviceName));
            }
        }

        await RegisterGroupedLights("room");
        await RegisterGroupedLights("zone");
    }

    async Task RegisterGroupedLights(string areaType1)
    {
        var areas = await _hueClient.GetResources(areaType1);
        foreach (var area in areas)
        {
            var roomName = area.Metadata?.Name ?? "Unknown area";

            var relatedService = area.Services.SingleOrDefault(s => s.RelatedType == "grouped_light");
            if (relatedService != null)
            {
                var group = await _hueClient.GetResource(relatedService.RelatedType, relatedService.RelatedId);
                var mqttTopic = $"{roomName}Lights".Pascalize();
                RegisterDevice(group, mqttTopic);
            }
        }
    }

    void RegisterDevice(HueResource resource, string mqttTopic)
    {
        if (!_mqttDevicesById.ContainsKey(resource.Id))
        {
            var mqttDevice = MqttDevice.CreateFrom(mqttTopic, resource);
            _mqttDevicesById[resource.Id] = mqttDevice;
        }
    }

    private static string CreateMqttTopic(HueResource resource, string mainDeviceName)
    {
        var nameParts = new List<string> { mainDeviceName.Pascalize() };

        var resourceType = resource.Type;

        if (resourceType == "button")
        {
            nameParts.Add($"Button{resource.Metadata!.ControlId}");
        }
        else if (resourceType != "light")
        {
            nameParts.Add(resourceType.Pascalize());
        }

        string name = string.Join("/", nameParts);
        return name;
    }

    async Task ProcessEventStream()
    {
        const string dataPrefix = "data: ";
        Console.WriteLine("Opening event stream");
        using var streamReader = new StreamReader(await _hueClient.GetEventStream());
        while (!streamReader.EndOfStream)
        {
            var message = await streamReader.ReadLineAsync();
            if (message == null) continue;
            if (!message.StartsWith(dataPrefix)) continue;

            var data = message.Substring(dataPrefix.Length);

            var eventStream = JsonSerializer.Deserialize<EventStream[]>(data);
            if (eventStream == null) continue;

            foreach (var @event in eventStream)
            {
                foreach (var hueResource in @event.Data)
                {
                    if (!_mqttDevicesById.ContainsKey(hueResource.Id)) continue;
                    var mqttDevice = _mqttDevicesById[hueResource.Id];
                    mqttDevice.UpdateFrom(hueResource);

                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        Converters =
                        {
                            new JsonStringEnumConverter()
                        },
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };

                    var json = JsonSerializer.Serialize(mqttDevice, mqttDevice.GetType(), options);

                    Console.WriteLine($"- {mqttDevice.Topic} - {json}");
                }
            }
        }
    }
}