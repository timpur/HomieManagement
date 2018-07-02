using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Serializer;
using Homie.Models;

namespace HomieManagement
{
    public class Configuration
    {
        public MqttOptions MQTT { get; set; }
        public List<string> HomieTopics { get; set; }
        public HomieOptions HomieOptions { get; set; }

        public Configuration()
        {
            HomieOptions = new HomieOptions();
        }


        public class MqttOptions
        {
            public string ClientId { get; set; }
            public bool CleanSession { get; set; }
            public TimeSpan KeepAlivePeriod { get; set; }
            public TimeSpan? KeepAliveSendInterval { get; set; }
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
                    KeepAliveSendInterval = KeepAliveSendInterval,
                    CommunicationTimeout = CommunicationTimeout,
                    ProtocolVersion = ProtocolVersion,
                    ChannelOptions = ServerOptions,
                    WillMessage = WillMessage
                };
            }
        }
    }
}
