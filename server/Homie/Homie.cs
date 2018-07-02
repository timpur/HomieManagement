using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using Homie.Models;
using Homie.Device;


namespace Homie
{
    public class Homie
    {
        public Dictionary<string, HomieDevice> Devices { get; }

        private IHomieMqttClient Client { get; }
        private SemaphoreSlim Lock = new SemaphoreSlim(1);


        public Homie(IHomieMqttClient client)
        {
            Devices = new Dictionary<string, HomieDevice>();
            Client = client;
        }

        public async Task Setup(List<string> homieTopics)
        {
            foreach (string baseTopic in homieTopics)
            {
                var topic = $"{baseTopic}/+/$homie";
                Helpers.BuildSubscriptionFromMqttTopic(topic, Client.ObservableMessages)
                  .Subscribe(async (message) => await DiscoverDevice(message));
                await Client.SubscribeAsync(topic);
            }
        }


        public async Task DiscoverDevice(IMqttMessage message)
        {
            var deviceOptions = Helpers.ProcessDeviceTopic(message.Topic, message.GetPayloadString());
            if (deviceOptions != null && !Devices.ContainsKey(deviceOptions.Id))
            {
                var device = new HomieDevice(deviceOptions.BaseTopic, deviceOptions.Id);
                await Lock.WaitAsync();
                Devices.Add(deviceOptions.Id, device);
                Lock.Release();
                await device.Setup(Client);
            }
        }
    }
}
