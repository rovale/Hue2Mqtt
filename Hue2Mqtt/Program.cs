using Hue2Mqtt;
using Hue2Mqtt.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;

var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "Hue2Mqtt.log");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Logger(config => { config.WriteTo.Console(); })
    .WriteTo.Logger(config => { config.WriteTo.File(logFile); })
    .CreateLogger();

Log.Information($"Log file: {logFile}");

var config = LocalConfiguration.BuildConfiguration(Directory.GetCurrentDirectory());
var applicationConfig = config.Get<ApplicationOptions>();

var hueBaseAddress = new Uri(applicationConfig.HueBaseAddress);
var hueKey = applicationConfig.HueKey;

var mqttServer = applicationConfig.MqttServer;
var mqttPort = applicationConfig.MqttPort;

await new Translator(new HueClient(hueBaseAddress, hueKey), new MqttClient(mqttServer, mqttPort)).Start();