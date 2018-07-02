using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Homie.Models;
using ObservableMqttMessage = System.IObservable<Homie.Models.IMqttMessage>;


namespace Homie.Device
{
    public abstract class DeviceBase : ObservableObject
    {

        public enum DiscoveryStage
        {
            Stage0,
            Stage1,
            Stage2,
            Stage3
        }

        public string BaseTopic { get; }
        public string DeviceId { get; }
        public string DeviceLevel { get { return $"{BaseTopic}/{DeviceId}"; } }

        protected BehaviorSubject<DiscoveryStage> DiscoveryStageSubject { get; }
        public IObservable<DiscoveryStage> ObservableDiscoveryStage { get { return DiscoveryStageSubject; } }


        public DeviceBase(string baseTopic, string id)
        {
            BaseTopic = baseTopic;
            DeviceId = id;

            DiscoveryStageSubject = new BehaviorSubject<DiscoveryStage>(DiscoveryStage.Stage0);
        }

        public abstract Task Setup(IHomieMqttClient client);
    }
}
