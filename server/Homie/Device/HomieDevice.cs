using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Homie.Models;


namespace Homie.Device
{
    public class HomieDevice : DeviceBase
    {
        public Dictionary<string, HomieNode> HomieNodes { get; }

        public string Version { get { return Get<string>(); } internal set { Set(value); } }


        public HomieDevice(string baseTopic, string id) : base(baseTopic, id)
        {
            HomieNodes = new Dictionary<string, HomieNode>();
            DiscoveryStageSubject.OnNext(DiscoveryStage.Stage1);
        }

        public async override Task Setup(IHomieMqttClient client)
        {
            Console.WriteLine($"Device Start: {DeviceId}");

            client.ObservableMessages
                .Where(message => message.Topic.StartsWith(DeviceLevel))
                .Subscribe(OnUpdate);

            // var topic = $"{DeviceLevel}/$nodes";
            // var subscription = Helpers.BuildSubscriptionFromMqttTopic(topic, client.ObservableMessages);
            // await client.SubscribeAsync(topic);
            // var discoveryMessage = await subscription.FirstAsync();
            var discoveryMessage = await Helpers.SubscribeAndWaitFirst($"{DeviceLevel}/$nodes", client);

            var tasks = new List<Task>();
            var nodeOptions = Helpers.ProcessNodesMessage(discoveryMessage.GetPayloadString())
              .Where(node => node != null && !HomieNodes.ContainsKey(node.Id));

            foreach (var nodeOption in nodeOptions)
            {
                var homieNode = new HomieNode(this, DeviceLevel, nodeOption);
                HomieNodes.Add(nodeOption.Id, homieNode);
                tasks.Add(homieNode.Setup(client));
            }
            await Task.WhenAll(tasks);
            DiscoveryStageSubject.OnNext(DiscoveryStage.Stage2);
            Console.WriteLine($"Device Done: {DeviceId}");
        }

        private void OnUpdate(IMqttMessage message)
        {
            switch (message.Topic.Replace(DeviceLevel, ""))
            {
                case "/$homie":
                    Version = message.GetPayloadString();

                    break;
            }
        }
    }
}
