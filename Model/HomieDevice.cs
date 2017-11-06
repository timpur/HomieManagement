using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using HomieManagement.Controllers;
using Microsoft.AspNetCore.SignalR;

namespace HomieManagement.Model
{
  public class HomieDevice : ChangeBase<HomieDevice>
  {
    private ILogger Logger { get; }
    private MQTTManager MQTTManager { get; }
    private IHubContext<HomieHub> HomieHub { get; }
    private string ConfigString { get; set; }
    private Listener Listener { get; set; }
    private Listener NodeListener { get; set; }

    [JsonProperty("hm_device_id")]
    public Guid HMDeviceID { get; private set; }
    [JsonProperty("mqtt_root_topic_level")]
    public string MQTTRootTopicLevel { get; private set; }
    [JsonProperty("version")]
    public string Version { get { return Get<string>(); } set { Set(value); } }
    [JsonProperty("status")]
    public bool? Status { get { return Get<bool?>(); } set { Set(value); } }
    [JsonProperty("name")]
    public string Name { get { return Get<string>(); } set { Set(value); } }
    [JsonProperty("local_ip")]
    public string LocalIP { get { return Get<string>(); } set { Set(value); } }
    [JsonProperty("mac")]
    public string MAC { get { return Get<string>(); } set { Set(value); } }
    [JsonProperty("uptime")]
    public string UpTime { get { return Get<string>(); } set { Set(value); } }
    [JsonProperty("signal")]
    public string Signal { get { return Get<string>(); } set { Set(value); } }
    [JsonProperty("update_interval")]
    public string UpdateInterval { get { return Get<string>(); } set { Set(value); } }
    [JsonProperty("fw_name")]
    public string FWName { get { return Get<string>(); } set { Set(value); } }
    [JsonProperty("fw_version")]
    public string FWVersion { get { return Get<string>(); } set { Set(value); } }
    [JsonProperty("fw_checksum")]
    public string FWCheckSum { get { return Get<string>(); } set { Set(value); } }
    [JsonProperty("implementation")]
    public string Implementation { get { return Get<string>(); } set { Set(value); } }
    [JsonProperty("implementation_version")]
    public string ImplementationVersion { get { return Get<string>(); } set { Set(value); } }
    [JsonProperty("config")]
    public DeviceConfig Config { get { return Get<DeviceConfig>(); } set { Set(value); } }
    [JsonProperty("ota_enabled")]
    public bool? OTAEnabled { get { return Get<bool?>(); } set { Set(value); } }
    [JsonProperty("ota_status")]
    public string OTAStatus { get { return Get<string>(); } set { Set(value); } }
    [JsonProperty("nodes")]
    public List<DeviceNode> Nodes { get; }


    public HomieDevice() : base(false)
    {
    }

    //[InjectionConstructor]
    public HomieDevice(MQTTManager mqttManger, IHubContext<HomieHub> homieHub, ILogger<HomieDevice> logger) : base(true)
    {
      HMDeviceID = Guid.NewGuid();
      Nodes = new List<DeviceNode>();

      Logger = logger;
      HomieHub = homieHub;
      MQTTManager = mqttManger;

      ChangeEvent += OnPropertyChange;
    }

    public void Setup(string roolLevel)
    {
      MQTTRootTopicLevel = roolLevel;

      Listener = new Listener($"{MQTTRootTopicLevel}/#", DevicePropertyHandler);
      MQTTManager.AddListner(Listener);

      NodeListener = new Listener($"{MQTTRootTopicLevel}/+/\\$type", DeviceNodeHandler);
      MQTTManager.AddListner(NodeListener);

    }

    //Private

    private void DevicePropertyHandler(SubscriptionMessage message)
    {
      var topic = message.Topic.Replace(MQTTRootTopicLevel, "");
      if (ProccessDeviceProperties(topic, message.Message))
      {
        message.Seen = true;
      }
    }

    private bool ProccessDeviceProperties(string topic, string message)
    {
      switch (topic)
      {
        case "/$homie":
          Version = message;
          break;
        case "/$online":
          Status = Boolean.Parse(message);
          break;
        case "/$name":
          Name = message;
          break;
        case "/$localip":
          LocalIP = message;
          break;
        case "/$mac":
          MAC = message;
          break;
        case "/$stats/uptime":
          UpTime = message;
          break;
        case "/$stats/signal":
          Signal = message;
          break;
        case "/$stats/interval":
          UpdateInterval = message;
          break;
        case "/$fw/name":
          FWName = message;
          break;
        case "/$fw/version":
          FWVersion = message;
          break;
        case "/$fw/checksum":
          FWCheckSum = message;
          break;
        case "/$implementation":
          Implementation = message;
          break;
        case "/$implementation/version":
          ImplementationVersion = message;
          break;
        case "/$implementation/config":
          ConfigString = message;
          Config = DeviceConfig.FromJSON(message);
          break;
        case "/$implementation/ota/enabled":
          OTAEnabled = Boolean.Parse(message); ;
          break;
        case "/$implementation/ota/status":
          OTAStatus = message; ;
          break;
        default:
          return false;
      }
      return true;
    }

    private void DeviceNodeHandler(SubscriptionMessage message)
    {
      var nodeRootTopic = message.Topic.Replace("/$type", "");
      var nodeID = nodeRootTopic.Replace(MQTTRootTopicLevel + "/", "");
      if (Nodes.Find(node => node.MQTTRootTopicLevel == nodeRootTopic) == null)
      {
        var node = new DeviceNode(nodeRootTopic, nodeID, MQTTManager);
        node.ChangeEvent += OnNodeChange;
        node.PropertyChangeEvent += OnNodePropertyChange;
        Nodes.Add(node);
      }

    }

    private void OnPropertyChange(HomieDevice device)
    {
      device.HMDeviceID = HMDeviceID;
      HomieHub.Clients.All.InvokeAsync("DeviceUpdate", device);
    }

    private void OnNodeChange(DeviceNode node)
    {
      HomieHub.Clients.All.InvokeAsync("DeviceNodeUpdate", HMDeviceID, node);
    }

    private void OnNodePropertyChange(DeviceNode node, NodeProperty property)
    {
      HomieHub.Clients.All.InvokeAsync("DeviceNodePropertyUpdate", HMDeviceID, node.NodeID, property);
    }

    private string GetConfigTopic()
    {
      return MQTTRootTopicLevel + "/$implementation/config";
    }

    private string GetResetTopic()
    {
      return MQTTRootTopicLevel + "/$implementation/reset";
    }

    private string GetFWChecksumTopic(string hash)
    {
      return MQTTRootTopicLevel + $"/$implementation/ota/firmware/{hash}";
    }

    private string GetFWStatusTopic()
    {
      return MQTTRootTopicLevel + $"/\\$implementation/ota/status";
    }

    //Public
    public bool UpDateConfig(DeviceConfig newConfig)
    {
      try
      {
        var change = DeviceConfig.CreateChangeObject(Config, newConfig);
        var json = change.ToJSON();
        if (json != "{}")
          MQTTManager.Publish(new PublishMessage(GetConfigTopic() + "/set", json)).Wait();
        return true;
      }
      catch (Exception ex)
      {
        Logger.LogError("An Error Occured: {0} \r\nStack: {1}", ex.Message, ex.StackTrace);
        return false;
      }
    }

    public bool Reset()
    {
      try
      {
        MQTTManager.Publish(new PublishMessage(GetResetTopic(), "true")).Wait();
        return true;
      }
      catch (Exception ex)
      {
        Logger.LogError("An Error Occured: {0} \r\nStack: {1}", ex.Message, ex.StackTrace);
        return false;
      }
    }

    public (bool sent, string code) SendFirmware(string checksum, string firmware)
    {
      try
      {
        var publishSuccess = MQTTManager.Publish(new PublishMessage(GetFWChecksumTopic(checksum), firmware)).Result;
        if (publishSuccess)
        {
          var code = MQTTManager.WaitForMessage(GetFWStatusTopic(), doSee: false, timeout: TimeSpan.FromSeconds(30)).Result;
          return (true, code?.Message ?? "Faild to recive status");
        }
        else
          return (false, "Faild to publish");
      }
      catch (Exception ex)
      {
        Logger.LogError("An Error Occured: {0} \r\nStack: {1}", ex.Message, ex.StackTrace);
        return (false, "An Error Occured");
      }
    }

    public override bool Equals(object obj)
    {
      if (obj is HomieDevice compare)
        return GetHashCode() == compare.GetHashCode();
      else
        return false;
    }

    public override int GetHashCode()
    {
      return
         (HMDeviceID != null ? HMDeviceID.GetHashCode() : 0) +
         (MQTTRootTopicLevel != null ? MQTTRootTopicLevel.GetHashCode() : 0) +
         (Version != null ? Version.GetHashCode() : 0) +
         (Status.GetHashCode()) +
         (Name != null ? Name.GetHashCode() : 0) +
         (LocalIP != null ? LocalIP.GetHashCode() : 0) +
         (MAC != null ? MAC.GetHashCode() : 0) +
         (UpTime != null ? UpTime.GetHashCode() : 0) +
         (Signal != null ? Signal.GetHashCode() : 0); //finish
    }


    public class DeviceNode : ChangeBase<DeviceNode>
    {
      private MQTTManager MQTTManager { get; }
      private Listener Listener { get; }

      public new event ChangedEventHandler<DeviceNode> ChangeEvent;
      public event ChangedParentEventHandler<DeviceNode, NodeProperty> PropertyChangeEvent;

      [JsonProperty("mqtt_root_topic_level")]
      public string MQTTRootTopicLevel { get; private set; }
      [JsonProperty("node_id")]
      public string NodeID { get; private set; }
      [JsonProperty("type")]
      public string Type { get { return Get<string>(); } set { Set(value); } }
      [JsonProperty("properties")]
      public List<NodeProperty> Properties { get; }

      public DeviceNode() : base(false)
      {

      }

      public DeviceNode(string roolLevel, string nodeID, MQTTManager mqttManager) : base(true)
      {
        Properties = new List<NodeProperty>();

        MQTTManager = mqttManager;

        MQTTRootTopicLevel = roolLevel;
        NodeID = nodeID;

        Listener = new Listener($"{MQTTRootTopicLevel}/#", NodePropertyHandler);
        MQTTManager.AddListner(Listener);

        base.ChangeEvent += DeviceNode_ChangeEvent;

      }

      private void DeviceNode_ChangeEvent(DeviceNode diff)
      {
        diff.NodeID = NodeID;
        ChangeEvent?.Invoke(diff);
      }

      private void NodePropertyHandler(SubscriptionMessage message)
      {
        var topic = message.Topic.Replace(MQTTRootTopicLevel, "");
        if (ProccessMQTTMessageForNodeProperties(topic, message.Message))
        {
          message.Seen = true;
        }
      }

      private bool ProccessMQTTMessageForNodeProperties(string topic, string message)
      {
        switch (topic)
        {
          case "/$type":
            Type = message;
            break;
          case "/$properties":
            var root = MQTTRootTopicLevel + topic;
            var properties = message.Split(',');
            foreach (var propertyString in properties)
            {
              var regex = new Regex("(?<Name>[A-Za-z1-9-]+)(?<Range>\\[(?<RangeFrom>[0-9]+)-(?<RangeTo>[0-9]+)\\])?(?<Settable>:settable)?");
              var match = regex.Match(propertyString);
              if (match.Success)
              {
                var name = match.Groups["Name"].Value;
                var range = match.Groups["Range"].Success;
                var settable = match.Groups["Settable"].Success;
                if (!range)
                {
                  AddProperty(root, name, settable);
                }
                else
                {
                  var from = Int32.Parse(match.Groups["RangeFrom"].Value);
                  var to = Int32.Parse(match.Groups["RangeTo"].Value);
                  for (int i = from; i <= to; ++i)
                  {
                    AddProperty(root, name + "_" + i, settable);
                  }
                }
              }
            }
            break;
          default:
            var propertyID = topic.Remove(0, 1);
            var property = Properties.Find(p => p.PropertyID == propertyID);
            if (property != null)
            {
              property.Value = message;
              return true;
            }
            return false;
        }
        return true;
      }

      private void AddProperty(string root, string propertyID, bool settable)
      {
        var prop = new NodeProperty(root + propertyID, propertyID, settable);
        prop.ChangeEvent += (diff) => PropertyChangeEvent?.Invoke(this, diff);
        Properties.Add(prop);
      }

    }

    public class NodeProperty : ChangeBase<NodeProperty>
    {
      public new event ChangedEventHandler<NodeProperty> ChangeEvent;

      [JsonProperty("mqtt_root_topic_level")]
      public string MQTTRootTopicLevel { get; private set; }
      [JsonProperty("property_id")]
      public string PropertyID { get; private set; }
      [JsonProperty("settable")]
      public bool? Settable { get { return Get<bool?>(); } set { Set(value); } }
      [JsonProperty("value")]
      public string Value { get { return Get<string>(); } set { Set(value); } }

      public NodeProperty() : base(false)
      {

      }

      public NodeProperty(string root, string propertyID, bool settable) : base(true)
      {
        MQTTRootTopicLevel = root;
        PropertyID = propertyID;
        Settable = settable;

        base.ChangeEvent += NodeProperty_ChangeEvent;
      }

      private void NodeProperty_ChangeEvent(NodeProperty diff)
      {
        diff.PropertyID = PropertyID;
        ChangeEvent?.Invoke(diff);
      }
    }

    public class DeviceConfig
    {
      [JsonProperty("name")]
      public string Name { get; set; }
      [JsonProperty("device_id")]
      public string DeviceID { get; set; }
      [JsonProperty("wifi")]
      public WiFi WiFi { get; set; }
      [JsonProperty("mqtt")]
      public MQTT MQTT { get; set; }
      [JsonProperty("ota")]
      public OTA OTA { get; set; }
      [JsonProperty("settings")]
      public Dictionary<string, object> Settings { get; set; }

      public DeviceConfig()
      {
        Settings = new Dictionary<string, object>();
      }

      public static DeviceConfig FromJSON(string configString)
      {
        return JsonConvert.DeserializeObject<DeviceConfig>(configString);
      }

      public string ToJSON()
      {
        return JsonConvert.SerializeObject(this, new JsonSerializerSettings
        {
          Formatting = Formatting.None,
          NullValueHandling = NullValueHandling.Ignore
        });
      }

      public override bool Equals(object obj)
      {
        if (obj is DeviceConfig compare)
          return GetHashCode() == compare.GetHashCode();
        else
          return false;
      }

      public override int GetHashCode()
      {
        return
           (Name != null ? Name.GetHashCode() : 0) +
           (DeviceID != null ? DeviceID.GetHashCode() : 0) +
           (WiFi != null ? WiFi.GetHashCode() : 0) +
           (MQTT != null ? MQTT.GetHashCode() : 0) +
           (OTA != null ? OTA.GetHashCode() : 0) +
           (Settings != null ? Settings.Select(item => (item.Key.GetHashCode() + item.Value.GetHashCode()).GetHashCode()).Sum(item => (long)item).GetHashCode() : 0);
      }

      public static DeviceConfig CreateChangeObject(DeviceConfig currentConfig, DeviceConfig newConfig)
      {
        var change = new DeviceConfig();
        if (currentConfig.Name != newConfig.Name)
          change.Name = newConfig.Name;
        if (currentConfig.DeviceID != newConfig.DeviceID)
          change.Name = newConfig.DeviceID;
        if (!currentConfig.WiFi.Equals(newConfig.WiFi))
          change.WiFi = WiFi.CreateChangeObject(currentConfig.WiFi, newConfig.WiFi);
        if (!currentConfig.MQTT.Equals(newConfig.MQTT))
          change.MQTT = MQTT.CreateChangeObject(currentConfig.MQTT, newConfig.MQTT);
        if (!currentConfig.OTA.Equals(newConfig.OTA))
          change.OTA = OTA.CreateChangeObject(currentConfig.OTA, newConfig.OTA);
        change.Settings = CreateChangeSettings(currentConfig.Settings, newConfig.Settings);

        return change;
      }

      private static Dictionary<string, object> CreateChangeSettings(Dictionary<string, object> currentSettings, Dictionary<string, object> newSettings)
      {
        var change = new Dictionary<string, object>();
        var changed = false;
        foreach (var item in newSettings)
        {
          if (currentSettings.ContainsKey(item.Key) && !currentSettings[item.Key].Equals(item.Value))
          {
            change.Add(item.Key, item.Value);
            changed = true;
          }
        }
        return changed ? change : null;
      }

    }

    public class WiFi
    {
      [JsonProperty("ssid")]
      public string SSID { get; set; }
      [JsonProperty("password")]
      public string Password { get; set; }
      [JsonProperty("bssid")]
      public string BSSID { get; set; }
      [JsonProperty("channel")]
      public int? Channel { get; set; }
      [JsonProperty("ip")]
      public string IP { get; set; }
      [JsonProperty("mask")]
      public string Mask { get; set; }
      [JsonProperty("gw")]
      public string GW { get; set; }
      [JsonProperty("dns1")]
      public string DNS1 { get; set; }
      [JsonProperty("dns2")]
      public string DNS2 { get; set; }

      public WiFi() { }

      public override bool Equals(object obj)
      {
        if (obj is WiFi compare)
          return GetHashCode() == compare.GetHashCode();
        else
          return false;
      }

      public override int GetHashCode()
      {
        return
           (SSID != null ? SSID.GetHashCode() : 0) +
           (Password != null ? Password.GetHashCode() : 0) +
           (BSSID != null ? BSSID.GetHashCode() : 0) +
           (Channel != null ? Channel.GetHashCode() : 0) +
           (IP != null ? IP.GetHashCode() : 0) +
           (Mask != null ? Mask.GetHashCode() : 0) +
           (GW != null ? GW.GetHashCode() : 0) +
           (DNS1 != null ? DNS1.GetHashCode() : 0) +
           (DNS2 != null ? DNS2.GetHashCode() : 0);
      }

      public static WiFi CreateChangeObject(WiFi currentWiFi, WiFi newWiFi)
      {
        var change = new WiFi();
        if (currentWiFi.SSID != newWiFi.SSID)
          change.SSID = newWiFi.SSID;
        if (currentWiFi.Password != newWiFi.Password)
          change.Password = newWiFi.Password;
        if (currentWiFi.BSSID != newWiFi.BSSID)
          change.BSSID = newWiFi.BSSID;
        if (currentWiFi.Channel != newWiFi.Channel)
          change.Channel = newWiFi.Channel;
        if (currentWiFi.IP != newWiFi.IP)
          change.IP = newWiFi.IP;
        if (currentWiFi.Mask != newWiFi.Mask)
          change.Mask = newWiFi.Mask;
        if (currentWiFi.GW != newWiFi.GW)
          change.GW = newWiFi.GW;
        if (currentWiFi.DNS1 != newWiFi.DNS1)
          change.DNS1 = newWiFi.DNS1;
        if (currentWiFi.DNS2 != newWiFi.DNS2)
          change.DNS2 = newWiFi.DNS2;
        return change;
      }
    }

    public class MQTT
    {
      [JsonProperty("host")]
      public string Host { get; set; }
      [JsonProperty("port")]
      public int? Port { get; set; }
      [JsonProperty("base_topic")]
      public string BaseTopic { get; set; }
      [JsonProperty("auth")]
      public bool? Auth { get; set; }
      [JsonProperty("username")]
      public string Username { get; set; }
      [JsonProperty("password")]
      public string Password { get; set; }

      public MQTT() { }

      public override bool Equals(object obj)
      {
        if (obj is MQTT compare)
          return GetHashCode() == compare.GetHashCode();
        else
          return false;
      }

      public override int GetHashCode()
      {
        return
           (Host != null ? Host.GetHashCode() : 0) +
           (Port != null ? Port.GetHashCode() : 0) +
           (BaseTopic != null ? BaseTopic.GetHashCode() : 0) +
           (Auth != null ? Auth.GetHashCode() : 0) +
           (Username != null ? Username.GetHashCode() : 0) +
           (Password != null ? Password.GetHashCode() : 0);
      }

      public static MQTT CreateChangeObject(MQTT currentMQTT, MQTT newMQTT)
      {
        var change = new MQTT();
        if (currentMQTT.Host != newMQTT.Host)
          change.Host = newMQTT.Host;
        if (currentMQTT.Port != newMQTT.Port)
          change.Port = newMQTT.Port;
        if (currentMQTT.BaseTopic != newMQTT.BaseTopic)
          change.BaseTopic = newMQTT.BaseTopic;
        if (currentMQTT.Auth != newMQTT.Auth)
          change.Auth = newMQTT.Auth;
        if (currentMQTT.Username != newMQTT.Username)
          change.Username = newMQTT.Username;
        if (currentMQTT.Password != newMQTT.Password)
          change.Password = newMQTT.Password;
        return change;
      }
    }

    public class OTA
    {
      [JsonProperty("enabled")]
      public bool? Enabled { get; set; }

      public OTA() { }

      public override bool Equals(object obj)
      {
        if (obj is OTA compare)
          return GetHashCode() == compare.GetHashCode();
        else
          return false;
      }

      public override int GetHashCode()
      {
        return
           (Enabled != null ? Enabled.GetHashCode() : 0);
      }

      public static OTA CreateChangeObject(OTA currentOTA, OTA newOTA)
      {
        var change = new OTA();
        if (currentOTA.Enabled != newOTA.Enabled)
          change.Enabled = newOTA.Enabled;
        return change;
      }
    }

  }

}
