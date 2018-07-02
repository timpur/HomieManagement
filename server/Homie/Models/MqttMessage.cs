using MQTTnet;


namespace Homie.Models
{

    public class MqttMessage : IMqttMessage
    {
        public string Topic { get; private set; }
        public byte[] Payload { get; private set; }
        public int QoSLevel { get; private set; }
        public bool Retain { get; private set; }

        internal static MqttMessage From(MqttApplicationMessage message)
        {
            return new MqttMessage
            {
                Topic = message.Topic,
                Payload = message.Payload,
                QoSLevel = (int)message.QualityOfServiceLevel,
                Retain = message.Retain
            };
        }

        public string GetPayloadString()
        {
            return Helpers.ConvertPayloadToString(Payload);
        }
    }
}
