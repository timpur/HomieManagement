using System.Text.RegularExpressions;

namespace Homie
{
    public static class Constants
    {
        // Regex
        public static Regex DISCOVER_DEVICE_FROM_TOPIC = new Regex(@"(?<base_topic>\w[-/\w]*\w)/(?<device_id>\w[-\w]*\w)/\$homie");
        public static Regex DISCOVER_NODES_FROM_PAYLOAD = new Regex(@"(?<node_id>\w[-/\w]*\w)");
        public static Regex DISCOVER_PROPERTIES_FROM_PAYLOAD = new Regex(@"(?<property_id>\w[-/\w]*\w)(\[(?<range_start>[0-9])-(?<range_end>[0-9]+)\])?(?<settable>:settable)?");

        // Global
        public static string DEFAULT_DISCOVERY_PREFIX = "homie";
        public static string HOMIE_SUPPORTED_VERSION = "2.0.1";
        public static int DEFAULT_QOS = 1;

        // Props
        public static string PROP_VALUE = "value";
        public static string PROP_UNIT = "unit";
        public static string PROP_ON = "on";
        public static string PROP_BRIGHTNESS = "brightness";
        public static string PROP_RGB = "rgb";

        // Values
        public static string STATE_UNKNOWN = "unknown";
        public static string STATE_ON = "true";
        public static string STATE_OFF = "false";
    }
}
