using System.Text.Json;
using Hue2Mqtt.HueApi;

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
        var resourceResponse = await JsonSerializer.DeserializeAsync<HueResources>(stream);
        var resources = resourceResponse!.data;
        return resources;
    }

    public async Task<Stream> GetEventStream()
    {
        return await _httpClient.GetStreamAsync(EventStreamUrl);
    }
}