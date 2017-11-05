using System;
using System.Collections.Generic;
using MQTTnet.Core.Client;

namespace HomieManagement.Config
{
  public class AppConfig
  {
    //MQTT
    public MqttClientTcpOptions MQTTConfig { get; set; }
    //Homie
    public List<string> RootDeviceTopicLevels { get; set; }

  }
}
