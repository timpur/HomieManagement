using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using Homie;
using Homie.Models;
using MQTTnet.Protocol;
using Microsoft.Extensions.Options;


namespace HomieManagement.MQTT
{
    public class MQTTManager
    {

        private Configuration Configuration { get; }
        public IMqttClient Client { get; }

        public MQTTManager(IOptions<Configuration> config)
        {
            Configuration = config.Value;
            Client = new MqttFactory().CreateMqttClient();
        }

        public async Task Connect()
        {
            await Client.ConnectAsync(Configuration.MQTT.Options());
            var homieClient = new HomieMqttClient(Client, Configuration.HomieOptions);
            var homie = new Homie.Homie(homieClient);

            // homieClient.ObservableMessages.Subscribe(message => Console.WriteLine($"{message.Topic} : {message.GetPayloadString()}"));

            await homie.Setup(Configuration.HomieTopics);
        }
    }
}


