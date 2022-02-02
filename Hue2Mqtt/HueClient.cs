using System.Net;
using System.Text.Json;
using Hue2Mqtt.State;
using Humanizer;

namespace Hue2Mqtt
{
    internal class HueClient
    {
        const string Key = "QdvOeyTvim778xIdkxT38cBEsx3wd5B4As1r5d9T";
        const string ResourceUrl = "https://192.168.178.200/clip/v2/resource";
        const string EventStreamUrl = "https://192.168.178.200/eventstream/clip/v2";
        readonly Dictionary<string, string> _mqttTopicsById = new();
        readonly Dictionary<string, MqttDevice> _mqttDevicesById = new();
        readonly Dictionary<string, HueResource[]> _servicesByType = new();

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
            Console.WriteLine("Getting all topics");
            var ignoredTypes = new[] { "bridge", "zigbee_connectivity", "entertainment" };

            var devices = await GetResources(client, "device");

            foreach (var device in devices)
            {
                var deviceName = device.metadata?.name ?? "Unknown device";

                foreach (var relatedService in device.services.Where(s => !ignoredTypes.Contains(s.rtype)))
                {
                    if (!_servicesByType.ContainsKey(relatedService.rtype))
                    {
                        _servicesByType[relatedService.rtype] = await GetResources(client, relatedService.rtype);
                    }

                    var services = _servicesByType[relatedService.rtype];

                    var service = services.Single(s => s.id == relatedService.rid);
                    RegisterTopicName(service, deviceName);
                }
            }

            await RegisterGroupedLights(client, "room");
            await RegisterGroupedLights(client, "zone");
        }

        async Task RegisterGroupedLights(HttpClient client, string areaType1)
        {
            var areas = await GetResources(client, areaType1);
            foreach (var area in areas)
            {
                var roomName = area.metadata?.name ?? "Unknown area";

                foreach (var relatedService in area.services.Where(s => s.rtype == "grouped_light"))
                {
                    _mqttTopicsById[relatedService.rid] = $"{roomName}Lights".Pascalize();
                }
            }
        }

        void RegisterTopicName(HueResource resource, string mainDeviceName)
        {
            var mqttTopic = CreateMqttTopic(resource, mainDeviceName);

            if (!_mqttTopicsById.ContainsKey(resource.id))
            {
                _mqttTopicsById[resource.id] = mqttTopic;
            }

            if (!_mqttDevicesById.ContainsKey(resource.id))
            {
                var mqttDevice = MqttDevice.CreateFrom(mqttTopic, resource);
                _mqttDevicesById[resource.id] = mqttDevice;
            }
        }

        private static string CreateMqttTopic(HueResource resource, string mainDeviceName)
        {
            var nameParts = new List<string>();

            nameParts.Add(mainDeviceName.Pascalize());

            var resourceType = resource.type;

            if (resourceType == "button")
            {
                nameParts.Add($"Button{resource.metadata.control_id}");
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
                        foreach (var datum in @event.data)
                        {
                            Console.WriteLine(_mqttTopicsById[datum.id]);
                        }
                    }
                }
            }
        }
    }
}
