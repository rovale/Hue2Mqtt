using Hue2Mqtt;
using Serilog;

const string key = "QdvOeyTvim778xIdkxT38cBEsx3wd5B4As1r5d9T";
Uri baseAddress = new("https://192.168.178.200/");

var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "Hue2Mqtt.log");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Logger(config => { config.WriteTo.Console(); })
    .WriteTo.Logger(config => { config.WriteTo.File(logFile); })
    .CreateLogger();

Log.Information($"Log file: {logFile}");

await new Translator(new HueClient(baseAddress, key), new MqttClient()).Start();