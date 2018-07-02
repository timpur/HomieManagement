using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Homie.Models;


namespace Homie.Device
{
    public class HomieProperty : DeviceBase
    {
        public HomieNode Node { get; }

        public bool Settable { get { return Get<bool>(); } internal set { Set(value); } }
        public PropertyRange Range { get { return Get<PropertyRange>(); } internal set { Set(value); } }
        public string State { get { return Get<string>(); } internal set { Set(value); } }


        public HomieProperty(HomieNode node, string baseTopic, DevicePropertyOptions options) : base(baseTopic, options.Id)
        {
            Node = node;
            Settable = options.Settable;
            Range = options.Range;
            DiscoveryStageSubject.OnNext(DiscoveryStage.Stage1);
        }

        public async override Task Setup(IHomieMqttClient client)
        {
            Console.WriteLine($"Property Start: {DeviceId}");

            await Task.Run(() =>
            {
                client.ObservableMessages
                .Where(message => message.Topic.StartsWith(DeviceLevel))
                .Subscribe(OnUpdate);

                DiscoveryStageSubject.OnNext(DiscoveryStage.Stage2);
                Console.WriteLine($"Property Done: {DeviceId}");
            });
        }

        private void OnUpdate(IMqttMessage message)
        {
            switch (message.Topic.Replace(DeviceLevel, ""))
            {
                case "":
                    State = message.GetPayloadString();
                    DiscoveryStageSubject.OnNext(DiscoveryStage.Stage3);
                    break;
            }
        }
    }
}
