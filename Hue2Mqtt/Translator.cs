﻿using Hue2Mqtt.HueApi;
using Hue2Mqtt.State;
using Serilog;

namespace Hue2Mqtt;

internal class Translator(HueClient hueClient, MqttClient mqttClient)
{
    private string _bridgeName = "bridge name";

    readonly Dictionary<string, MqttDevice> _mqttDevicesById = new();

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

        await mqttClient.Connect();

        foreach (var mqttDevice in _mqttDevicesById.Values.OrderBy(m => m.Topic))
        {
            await mqttClient.Publish(_bridgeName, mqttDevice);
        }

        while (true)
        {
            try
            {
                await hueClient.ProcessEventStream(OnChange);
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

    private async Task RegisterDevices()
    {
        Log.Information("Registering devices");
        var ignoredTypes = new[] { "bridge", "zigbee_connectivity", "entertainment", "device_software_update", "zigbee_device_discovery" };

        var devices = await hueClient.GetResources("device");

        _bridgeName = devices
            .SingleOrDefault(d => d.Metadata?.Archetype == "bridge_v2")?
            .Metadata?.Name ?? _bridgeName;

        foreach (var device in devices)
        {
            var deviceName = device.Metadata?.Name ?? "Unknown device";

            var relatedServices = 
                await Task.WhenAll(device.
                    Services.Where(s => !ignoredTypes.Contains(s.RelatedType))
                    .ToArray()
                    .Select(GetRelatedService));

            if (relatedServices.Length > 0)
            {
                RegisterMqttDevice(device, relatedServices, mqttClient.CreateMqttTopic(device.Type, deviceName));
            }
        }

        await RegisterGroupedLights("room");
        await RegisterGroupedLights("zone");
    }

    private async Task RegisterGroupedLights(string areaType)
    {
        var areas = await hueClient.GetResources(areaType);
        foreach (var area in areas)
        {
            var roomName = area.Metadata?.Name ?? "Unknown area";

            var relatedServices =
                await Task.WhenAll(area.
                    Services.Where(s => s.RelatedType == "grouped_light") //TODO: check if there is more
                    .ToArray()
                    .Select(GetRelatedService));

            RegisterMqttDevice(area, relatedServices, mqttClient.CreateMqttTopic(areaType, $"{roomName}Lights"));
        }
    }

    private async Task<HueResource> GetRelatedService(Service relatedService)
    {
        var relatedServiceType = relatedService.RelatedType;
        var relatedServiceId = relatedService.RelatedId;
        var resource = await hueClient.GetResource(relatedServiceType, relatedServiceId);
        return resource;
    }

    private void RegisterMqttDevice(HueResource deviceOrGroup, HueResource[] relatedServices, string mqttTopic)
    {
        if (!_mqttDevicesById.ContainsKey(deviceOrGroup.Id))
        {
            _mqttDevicesById[deviceOrGroup.Id] = new MqttDevice(mqttTopic);
        }

        var mqttDevice = _mqttDevicesById[deviceOrGroup.Id];
        mqttDevice.UpdateFrom(deviceOrGroup);
        foreach (var relatedService in relatedServices)
        {
            mqttDevice.UpdateFrom(relatedService);
        }
    }

    private async Task OnChange(HueResource hueResource)
    {
        var devices = await hueClient.GetResources("device");
        var zones = await hueClient.GetResources("zone");
        var rooms = await hueClient.GetResources("room");

        var all = devices.Union(rooms).Union(zones);

        var deviceOrGroup =
            all.FirstOrDefault(d =>
                d.Services.Any(s => s.RelatedId == hueResource.Id));

        if (deviceOrGroup == null) return;
        if (!_mqttDevicesById.TryGetValue(deviceOrGroup.Id, out var mqttDevice)) return;

        mqttDevice.UpdateFrom(hueResource);
        await mqttClient.Publish(_bridgeName, mqttDevice);
    }
}