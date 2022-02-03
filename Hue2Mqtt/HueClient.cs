using System.Net;
using System.Text.Json;
using Hue2Mqtt.HueApi;
using Hue2Mqtt.State;
using Humanizer;

namespace Hue2Mqtt
{
    internal class HueClient
    {
        const string Key = "QdvOeyTvim778xIdkxT38cBEsx3wd5B4As1r5d9T";
        const string ResourceUrl = "https://192.168.178.200/clip/v2/resource";
        const string EventStreamUrl = "https://192.168.178.200/eventstream/clip/v2";
        readonly Dictionary<string, MqttDevice> _mqttDevicesById = new();
        readonly Dictionary<string, HueResource[]> _resourcesByType = new();

        public async Task Start()
        {
            using var client = GetHttpClient(Key);

            await RegisterDevices(client);
            while (true)
            {
                try
                {
                    await ProcessEventStream(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Retrying in 5 seconds");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

        HttpClient GetHttpClient(string s)
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            var httpClient = new HttpClient(httpClientHandler);
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            httpClient.DefaultRequestHeaders.Add("hue-application-key", s);
            httpClient.DefaultRequestHeaders.Add("Accept", "text/event-stream");
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            return httpClient;
        }

        async Task RegisterDevices(HttpClient client)
        {
            Console.WriteLine("Registering devices");
            var ignoredTypes = new[] { "bridge", "zigbee_connectivity", "entertainment" };

            var devices = await GetResources(client, "device");

            foreach (var device in devices)
            {
                var deviceName = device.Metadata?.Name ?? "Unknown device";

                foreach (var relatedService in device.Services.Where(s => !ignoredTypes.Contains(s.RelatedType)))
                {
                    var relatedServiceType = relatedService.RelatedType;
                    var relatedServiceId = relatedService.RelatedId;

                    var service = await GetHueResource(client, relatedServiceType, relatedServiceId);
                    RegisterDevice(service, CreateMqttTopic(service, deviceName));
                }
            }

            await RegisterGroupedLights(client, "room");
            await RegisterGroupedLights(client, "zone");
        }

        private async Task<HueResource> GetHueResource(HttpClient client, string type, string id)
        {
            if (!_resourcesByType.ContainsKey(type))
            {
                _resourcesByType[type] = await GetResources(client, type);
            }

            var resources = _resourcesByType[type];
            var resource = resources.Single(r => r.Id == id);
            return resource;
        }

        async Task RegisterGroupedLights(HttpClient client, string areaType1)
        {
            var areas = await GetResources(client, areaType1);
            foreach (var area in areas)
            {
                var roomName = area.Metadata?.Name ?? "Unknown area";

                var relatedService = area.Services.SingleOrDefault(s => s.RelatedType == "grouped_light");
                if (relatedService != null)
                {
                    var group = await GetHueResource(client, relatedService.RelatedType, relatedService.RelatedId);
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
            var nameParts = new List<string>();

            nameParts.Add(mainDeviceName.Pascalize());

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

        async Task<HueResource[]> GetResources(HttpClient client, string resourceName)
        {
            var url = $"{ResourceUrl}/{resourceName}";
            var stream = await client.GetStreamAsync(url);
            var resourceResponse = await JsonSerializer.DeserializeAsync<HueResources>(stream);
            var resources = resourceResponse.data;
            return resources;
        }

        async Task ProcessEventStream(HttpClient client)
        {
            Console.WriteLine("Opening event stream");
            using var streamReader = new StreamReader(await client.GetStreamAsync(EventStreamUrl));
            while (!streamReader.EndOfStream)
            {
                var message = await streamReader.ReadLineAsync();
                if (message.StartsWith("data"))
                {
                    var data = message.Substring("data: ".Length);
                    Console.WriteLine(data.JsonPrettify());

                    var eventStream = JsonSerializer.Deserialize<EventStream[]>(data);
                    foreach (var @event in eventStream)
                    {
                        foreach (var datum in @event.Data)
                        {
                            if (_mqttDevicesById.ContainsKey(datum.Id))
                            {
                                Console.WriteLine(_mqttDevicesById[datum.Id].Topic);
                            }
                        }
                    }
                }
            }
        }
    }
}
