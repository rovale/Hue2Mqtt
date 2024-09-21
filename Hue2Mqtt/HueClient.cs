using System.Text.Json;
using Hue2Mqtt.HueApi;
using Serilog;

namespace Hue2Mqtt;

internal class HueClient
{
    const string ResourceUrl = "clip/v2/resource";
    const string EventStreamUrl = "eventstream/clip/v2";
    readonly Dictionary<string, HueResource[]> _resourcesByType = new();

    private readonly HttpClient _httpClient;

    public HueClient(Uri baseAddress, string key)
    {
        _httpClient = GetHttpClient(baseAddress, key);
    }

    private HttpClient GetHttpClient(Uri baseAddress, string key)
    {
        var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        var httpClient = new HttpClient(httpClientHandler)
        {
            BaseAddress = baseAddress,
            Timeout = TimeSpan.FromSeconds(5)
        };
        httpClient.DefaultRequestHeaders.Add("hue-application-key", key);
        httpClient.DefaultRequestHeaders.Add("Accept", "text/event-stream");

        return httpClient;
    }

    public async Task<HueResource> GetResource(string type, string id)
    {
        if (!_resourcesByType.ContainsKey(type))
        {
            _resourcesByType[type] = await GetResources(type);
        }

        var resources = _resourcesByType[type];
        var resource = resources.Single(r => r.Id == id);
        return resource;
    }

    public async Task<HueResource[]> GetResources(string type)
    {
        var url = $"{ResourceUrl}/{type}";
        var stream = await _httpClient.GetStreamAsync(url);

        using var streamReader = new StreamReader(stream);
        var jsonString = await streamReader.ReadToEndAsync();
        //Log.Debug($"Type: {type} - ${jsonString}");
        var resourceResponse = JsonSerializer.Deserialize<HueResources>(jsonString);
        var resources = resourceResponse!.Data;
        return resources;
    }

    public async Task ProcessEventStream(Func<HueResource,Task> onChange)
    {
        Log.Information("Opening event stream");
        using var streamReader = new StreamReader(await _httpClient.GetStreamAsync(EventStreamUrl));

        const string dataPrefix = "data: ";
        while (!streamReader.EndOfStream)
        {
            var message = await streamReader.ReadLineAsync();
            if (message == null) continue;
            if (!message.StartsWith(dataPrefix)) continue;

            var eventsJson = message.Substring(dataPrefix.Length);

            var events = JsonSerializer.Deserialize<Events[]>(eventsJson);
            if (events == null) continue;

            foreach (var @event in events)
            {
                foreach (var data in @event.Data)
                {
                    await onChange(data);
                }
            }
        }
    }
}
