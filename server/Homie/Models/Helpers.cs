using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Homie.Device;
using Homie.Models;

using ObservableMqttMessage = System.IObservable<Homie.Models.IMqttMessage>;


namespace Homie
{
    public static class Helpers
    {
        public static string ConvertPayloadToString(byte[] payload)
        {
            return Encoding.UTF8.GetString(payload, 0, payload.Length);
        }

        public static byte[] ConvertPayloadToBytes(string payload)
        {
            return Encoding.UTF8.GetBytes(payload);
        }

        public static DeviceOptions ProcessDeviceTopic(string topic, string version)
        {
            var match = Constants.DISCOVER_DEVICE_FROM_TOPIC.Match(topic);

            if (match.Success && version == Constants.HOMIE_SUPPORTED_VERSION)
            {
                var baseTopic = match.Groups["base_topic"].Value;
                var id = match.Groups["device_id"].Value;

                return new DeviceOptions(baseTopic, id, version);
            }

            return null;
        }

        public static List<DeviceNodeOptions> ProcessNodesMessage(string message)
        {
            var nodes = message.Split(',');
            return nodes
            .Select(ProcessNodeMessage)
            .ToList();
        }

        private static DeviceNodeOptions ProcessNodeMessage(string nodeString)
        {
            var match = Constants.DISCOVER_NODES_FROM_PAYLOAD.Match(nodeString);

            if (match.Success)
            {
                var id = match.Groups["node_id"].Value;

                return new DeviceNodeOptions(id);
            }
            return null;
        }

        public static List<DevicePropertyOptions> ProcessPropertiesMessage(string message)
        {
            var properties = message.Split(',');

            return properties
            .Select(ProcessPropertyMessage)
            .ToList();
        }

        private static DevicePropertyOptions ProcessPropertyMessage(string propertyString)
        {
            var match = Constants.DISCOVER_PROPERTIES_FROM_PAYLOAD.Match(propertyString);
            if (match.Success)
            {
                var id = match.Groups["property_id"].Value;
                var settable = match.Groups["settable"].Success;
                var start = match.Groups["range_start"].Value;
                var end = match.Groups["range_end"].Value;

                return new DevicePropertyOptions(id, settable, start, end);
            }

            return null;
        }

        public static Regex ConvertMqttTopicToRegex(string topic)
        {
            var pattern = topic
              .Replace("/", "\\/")
              .Replace("+", ".*")
              .Replace("#", ".*")
              .Replace("$", "\\$");

            return new Regex(pattern);
        }

        public static ObservableMqttMessage BuildSubscriptionFromMqttTopic(string topic, ObservableMqttMessage messages)
        {
            var regexTopic = Helpers.ConvertMqttTopicToRegex(topic);
            return messages.Where(message => regexTopic.Match(message.Topic).Success);
        }

        public static async Task<IMqttMessage> SubscribeAndWaitFirst(string topic, IHomieMqttClient client)
        {
            var subTask = Task.Run(async () => await client.SubscribeAsync(topic));
            var message = await BuildSubscriptionFromMqttTopic(topic, client.ObservableMessages).FirstAsync();
            await subTask;
            return message;
        }
    }
}
