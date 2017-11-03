using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HomieManagement.Config
{
    public class AppConfig
    {
        //MQTT
        public string MqttHostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        //Homie
        public List<string> RootDeviceTopicLevels { get; set; }

    }
}
