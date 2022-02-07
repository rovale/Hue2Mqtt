using Hue2Mqtt;

const string key = "QdvOeyTvim778xIdkxT38cBEsx3wd5B4As1r5d9T";
Uri baseAddress = new("https://192.168.178.200/");

await new Translator(new HueClient(baseAddress, key), new MqttClient()).Start();