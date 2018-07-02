using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Homie.Models;


namespace Homie.Device
{
    public class HomieNode : DeviceBase
    {
        public HomieDevice Device { get; }
        public Dictionary<string, HomieProperty> HomieProperties { get; }

        public string Type { get { return Get<string>(); } internal set { Set(value); } }


        public HomieNode(HomieDevice device, string baseTopic, DeviceNodeOptions options) : base(baseTopic, options.Id)
        {
            Device = device;
            HomieProperties = new Dictionary<string, HomieProperty>();
            DiscoveryStageSubject.OnNext(DiscoveryStage.Stage1);
        }

        public async override Task Setup(IHomieMqttClient client)
        {
            Console.WriteLine($"Node Start: {DeviceId}");

            client.ObservableMessages
              .Where(message => message.Topic.StartsWith(DeviceLevel))
              .Subscribe(OnUpdate);

            // var topic = $"{DeviceLevel}/$properties";
            // var subscription = Helpers.BuildSubscriptionFromMqttTopic(topic, client.ObservableMessages);
            // await client.SubscribeAsync(topic);
            // var discoveryMessage = await subscription.FirstAsync();
            var discoveryMessage = await Helpers.SubscribeAndWaitFirst($"{DeviceLevel}/$properties", client);

            var tasks = new List<Task>();
            var propertyOptions = Helpers.ProcessPropertiesMessage(discoveryMessage.GetPayloadString())
              .Where(property => property != null && !HomieProperties.ContainsKey(property.Id));

            foreach (var propertyOption in propertyOptions)
            {
                var homieProperty = new HomieProperty(this, DeviceLevel, propertyOption);
                HomieProperties.Add(propertyOption.Id, homieProperty);
                tasks.Add(homieProperty.Setup(client));
            }
            await Task.WhenAll(tasks);
            DiscoveryStageSubject.OnNext(DiscoveryStage.Stage2);

            Console.WriteLine($"Node Done: {DeviceId}");
        }

        private void OnUpdate(IMqttMessage message)
        {
            switch (message.Topic.Replace(DeviceLevel, ""))
            {
                case "/$type":
                    Type = message.GetPayloadString();

                    break;
            }
        }
    }
}
