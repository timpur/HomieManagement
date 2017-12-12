using System;
using System.Collections.Generic;
using MQTTnet.Core;
using MQTTnet.Core.Client;
using MQTTnet.Core.Serializer;

namespace HomieManagement.Config
{
  public class AppConfig
  {
    //MQTT
    public MQTTConfig_TCP MQTTConfig { get; set; }
    //Homie
    public List<string> RootDeviceTopicLevels { get; set; }

  }

  public class MQTTConfig_TCP
  {

    public string ClientId { get; set; }
    public bool CleanSession { get; set; }
    public TimeSpan KeepAlivePeriod { get; set; }
    public TimeSpan CommunicationTimeout { get; set; }
    public MqttProtocolVersion ProtocolVersion { get; set; }
    public MqttClientTcpOptions ServerOptions { get; set; }
    public MqttClientCredentials Credentials { get; set; }
    public MqttApplicationMessage WillMessage { get; set; }

    public IMqttClientOptions Options()
    {
      return new MqttClientOptions()
      {
        ClientId = ClientId,
        CleanSession = CleanSession,
        Credentials = Credentials,
        KeepAlivePeriod = KeepAlivePeriod,
        CommunicationTimeout = CommunicationTimeout,
        ProtocolVersion = ProtocolVersion,
        ChannelOptions = ServerOptions,
        WillMessage = WillMessage
      };
    }
  }

}
