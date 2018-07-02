using System;

namespace Homie.Device
{
    public class DeviceOptions
    {
        public string BaseTopic { get; }
        public string Id { get; }
        public string Version { get; }

        public DeviceOptions(string baseTopic, string id, string version)
        {
            BaseTopic = baseTopic;
            Id = id;
            Version = version;
        }
    }

    public class DeviceNodeOptions
    {
        public string Id { get; }

        public DeviceNodeOptions(string id)
        {
            Id = id;
        }
    }

    public class DevicePropertyOptions
    {
        public string Id { get; }
        public bool Settable { get; }
        public PropertyRange Range { get; }

        public DevicePropertyOptions(string id, bool settable, string start, string end)
        {
            Id = id;
            Settable = settable;
            if (!String.IsNullOrEmpty(start) && !String.IsNullOrEmpty(end)) Range = new PropertyRange(start, end);
        }
    }

    public class PropertyRange
    {
        public int Start { get; }
        public int End { get; }

        public PropertyRange(string start, string end)
        {
            Start = int.Parse(start);
            End = int.Parse(end);
        }
    }
}
