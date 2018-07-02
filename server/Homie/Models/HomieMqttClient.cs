using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Protocol;
using ObservableMqttMessage = System.IObservable<Homie.Models.IMqttMessage>;


namespace Homie.Models
{
    public class HomieMqttClient : IHomieMqttClient
    {
        public ObservableMqttMessage ObservableMessages { get; }

        private IMqttClient Client { get; }
        private HomieOptions Options { get; }


        public HomieMqttClient(IMqttClient client, HomieOptions options)
        {
            Client = client;
            Options = options;
            ObservableMessages = GetMessageObservable(Client);
        }

        public async Task<bool> PublishAsync(string topic, string message)
        {
            try
            {
                var mqttMessage = new MqttApplicationMessage()
                {
                    Topic = topic,
                    Payload = Helpers.ConvertPayloadToBytes(message),
                    QualityOfServiceLevel = (MqttQualityOfServiceLevel)Options.QoSLevelPublish,
                    Retain = Options.Retain
                };
                await Client.PublishAsync(new MqttApplicationMessage[] { mqttMessage });
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return false;
            }
            return true;
        }

        public async Task<bool> SubscribeAsync(string topic)
        {
            try
            {
                var result = await Client.SubscribeAsync(topic, (MqttQualityOfServiceLevel)Options.QoSLevelSubscribe);
                if (result[0].ReturnCode == MqttSubscribeReturnCode.Failure) return false;
                return true;
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return false;
            }
        }



        public static IObservable<IMqttMessage> GetMessageObservable(IMqttClient client)
        {
            return Observable.Create<IMqttMessage>(observer =>
              {
                  var disposableMessage = Observable.FromEventPattern<MqttApplicationMessageReceivedEventArgs>(
                    h => client.ApplicationMessageReceived += h,
                    h => client.ApplicationMessageReceived -= h
                  )
                  .Select(messageEvent => messageEvent.EventArgs.ApplicationMessage)
                  .Subscribe(message =>
                    {
                        observer.OnNext(MqttMessage.From(message));
                    },
                    observer.OnError
                  );

                  var disposableDisconnect = Observable.FromEventPattern<MqttClientDisconnectedEventArgs>(
                    h => client.Disconnected += h,
                    h => client.Disconnected -= h
                  )
                  .Subscribe(disconnectEvent =>
                    {
                        if (!client.IsConnected) return;
                        observer.OnCompleted();
                    },
                    observer.OnError
                  );

                  return new CompositeDisposable(
                            disposableMessage,
                            disposableDisconnect
                  );
              }
            );
        }
    }
}
