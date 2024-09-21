using Hue2Mqtt.HueApi;
using Hue2Mqtt.State;
using Serilog;

namespace Hue2Mqtt;

internal class Translator
{
    private readonly HueClient _hueClient;
    private readonly MqttClient _mqttClient;
    private string _bridgeName = "bridge name";

    readonly Dictionary<string, MqttDevice> _mqttDevicesById = new();

    public Translator(HueClient hueClient, MqttClient mqttClient)
    {
        _hueClient = hueClient;
        _mqttClient = mqttClient;
    }

    public async Task Start()
    {
        try
        {
            await RegisterDevices();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred registering the devices");
            throw;
        }

        await _mqttClient.Connect();

        while (true)
        {
            try
            {
                await _hueClient.ProcessEventStream(OnChange);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred processing the Hue event stream");
                Log.Information("Retrying in 5 seconds");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }

    async Task RegisterDevices()
    {
        Log.Information("Registering devices");
        var ignoredTypes = new[] { "bridge", "zigbee_connectivity", "entertainment", "device_software_update", "zigbee_device_discovery" };

        var devices = await _hueClient.GetResources("device");
        RegisterBridge(devices);

        foreach (var device in devices)
        {
            var deviceName = device.Metadata?.Name ?? "Unknown device";

            foreach (var relatedService in device.Services.Where(s => !ignoredTypes.Contains(s.RelatedType)))
            {
                var relatedServiceType = relatedService.RelatedType;
                var relatedServiceId = relatedService.RelatedId;

                var resource = await _hueClient.GetResource(relatedServiceType, relatedServiceId);
                await RegisterDevice(resource, CreateMqttTopic(resource, deviceName));
            }
        }

        await RegisterGroupedLights("room");
        await RegisterGroupedLights("zone");
    }

    private void RegisterBridge(HueResource[] devices)
    {
        _bridgeName = devices
            .SingleOrDefault(d => d.Metadata?.Archetype == "bridge_v2")?
            .Metadata?.Name ?? _bridgeName;
    }

    async Task RegisterGroupedLights(string areaType)
    {
        var areas = await _hueClient.GetResources(areaType);
        foreach (var area in areas)
        {
            var roomName = area.Metadata?.Name ?? "Unknown area";

            var relatedService = area.Services.SingleOrDefault(s => s.RelatedType == "grouped_light");
            if (relatedService != null)
            {
                var group = await _hueClient.GetResource(relatedService.RelatedType, relatedService.RelatedId);
                var mqttTopic = _mqttClient.CreateMqttTopic($"{roomName}Lights");
                RegisterDevice(group, mqttTopic);
            }
        }
    }

    async Task RegisterDevice(HueResource resource, string mqttTopic)
    {
        if (!_mqttDevicesById.ContainsKey(resource.Id))
        {
            var mqttDevice = MqttDevice.CreateFrom(mqttTopic, resource);
            if (mqttDevice != null)
            {
                _mqttDevicesById[resource.Id] = mqttDevice;
            }
            else
            {
                await _mqttClient.Publish(_bridgeName, "system", "resource type " + resource.Type + " is not known.");
            }
        }
    }

    private string CreateMqttTopic(HueResource resource, string mainDeviceName)
    {
        List<string> nameParts = [mainDeviceName];

        var resourceType = resource.Type;

        if (resourceType == "button")
        {
            nameParts.Add($"Button{resource.Metadata!.ControlId}");
        }
        else if (resourceType != "light")
        {
            nameParts.Add(resourceType);
        }

        return _mqttClient.CreateMqttTopic(nameParts.ToArray());
    }

    private async Task OnChange(HueResource hueResource)
    {
        if (!_mqttDevicesById.ContainsKey(hueResource.Id)) return;
        var mqttDevice = _mqttDevicesById[hueResource.Id];
        mqttDevice.UpdateFrom(hueResource);
        await _mqttClient.Publish(_bridgeName, mqttDevice);
    }
}