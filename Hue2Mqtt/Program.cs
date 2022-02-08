using System.ComponentModel.DataAnnotations;
using Hue2Mqtt;
using Hue2Mqtt.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;

// https://www.roundthecode.com/dotnet/how-to-read-the-appsettings-json-configuration-file-in-asp-net-core
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("AppSettings.json", optional: false, reloadOnChange: true)
    .Build();

var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "Hue2Mqtt-.log");
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config, sectionName: "Logging")
    .WriteTo.Logger(c => { c.WriteTo.Console(); })
    .WriteTo.Logger(c => { c.WriteTo.File(logFile, rollingInterval: RollingInterval.Day); })
    .CreateLogger();

var appSettings = config.GetSection("Hue2Mqtt").Get<AppSettings>();
var validationResults = new List<ValidationResult>();
var valid = Validator.TryValidateObject(appSettings, new ValidationContext(appSettings), validationResults, true);

if (!valid)
{
    foreach (var validationResult in validationResults)
    {
        Log.Error(validationResult.ErrorMessage);
    }

    return;
}

var hueBaseAddress = new Uri(appSettings.HueBaseAddress);
var hueKey = appSettings.HueKey;

var mqttServer = appSettings.MqttServer;
var mqttPort = appSettings.MqttPort;

await new Translator(new HueClient(hueBaseAddress, hueKey), new MqttClient(mqttServer, mqttPort)).Start();