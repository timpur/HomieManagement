using System;
using System.Threading.Tasks;

using ObservableMqttMessage = System.IObservable<Homie.Models.IMqttMessage>;


namespace Homie.Models
{
    public interface IHomieMqttClient
    {
        ObservableMqttMessage ObservableMessages { get; }
        Task<bool> SubscribeAsync(string topic);
        Task<bool> PublishAsync(string topic, string message);
    }
}
