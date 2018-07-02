

namespace Homie.Models
{
    public interface IMqttMessage
    {
        string Topic { get; }
        byte[] Payload { get; }
        int QoSLevel { get; }
        bool Retain { get; }

        string GetPayloadString();
    }
}
