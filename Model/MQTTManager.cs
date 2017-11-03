using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using HomieManagement.Config;

namespace HomieManagement.Model
{
  public delegate void onMQTTMessage(MQTTMessage topic);
  public delegate void SubscribeCallback(SubscriptionMessage message);

  public class MQTTManager
  {
    private ILogger Logger { get; }

    private AppConfig Config { get; }
    private Timer Timer { get; }

    private MqttClient Client { get; set; }
    private bool InConnecting { get; set; }

    private ConcurrentDictionary<Guid, SubscriptionMessage> Messages { get; }
    private bool NewListeners { get; set; }
    private ConcurrentDictionary<Guid, Listener> Listeners { get; }


    public MQTTManager(IOptions<AppConfig> config, ILogger<MQTTManager> logger)
    {
      Logger = logger;
      Config = config.Value;

      Timer = new Timer(CheckConnect, null, 10000, 10000);
      Messages = new ConcurrentDictionary<Guid, SubscriptionMessage>();
      NewListeners = false;
      Listeners = new ConcurrentDictionary<Guid, Listener>();
    }

    public bool Connect()
    {
      var status = false;
      try
      {
        if (!InConnecting)
        {
          InConnecting = true;

          Client = new MqttClient(Config.MqttHostName, Config.Port, false, null, null, MqttSslProtocols.None);
          Client.MqttMsgPublishReceived += OnMqttMessage;
          byte result = Client.Connect("HomieManagement", Config.UserName, Config.Password, true, MqttSettings.MQTT_DEFAULT_TIMEOUT);

          if (result == 0)
          {
            Subscribe();
            status = true;
          }
        }
      }
      catch (Exception ex)
      {
        Logger.LogError("An Error Occured: {0} \r\nStack: {1}", ex.Message, ex.StackTrace);
      }
      InConnecting = false;
      return status;
    }

    //Private
    private void CheckConnect(object state)
    {
      if (Client != null && !Client.IsConnected)
      {
        Connect();
        ProccessMessage();
      }
    }

    private void OnMqttMessage(object sender, MqttMsgPublishEventArgs e)
    {
      try
      {
        AddMessage(new SubscriptionMessage(e.Topic, BytesToString(e.Message), e.QosLevel, e.Retain, e.DupFlag));
        ProccessMessage();
      }
      catch (Exception ex)
      {
        Logger.LogError("An Error Occured: {0} \r\nStack: {1}", ex.Message, ex.StackTrace);
      }
    }

    private void Subscribe()
    {
      foreach (var sub in Config.RootDeviceTopicLevels)
      {
        var topic = sub + "#";
        Subscribe(topic, 0);
      }
    }

    private void Subscribe(string topic, byte qos)
    {
      Client.Subscribe(new string[] { topic }, new byte[] { qos });
    }

    private void ProccessMessage()
    {
      var toRemoveMessages = new List<Guid>();
      var toRemoveListeners = new List<Guid>();

      var messages = Messages.Where(item => !item.Value.Seen);
      foreach (var message in messages)
      {
        var listeners = Listeners.Where(item => item.Value.MatchTopic(message.Value.Topic));
        foreach (var listener in listeners)
        {
          listener.Value.Callback(message.Value);
          if (listener.Value.Once) toRemoveListeners.Add(listener.Key);
        }
        if (message.Value.Seen) toRemoveMessages.Add(message.Key);

      }

      toRemoveMessages.ForEach(item => RemoveMessage(item));
      toRemoveListeners.ForEach(item => RemoveListner(item));

      if (NewListeners)
      {
        NewListeners = false;
        ProccessMessage();
      }
    }

    private void AddMessage(SubscriptionMessage message)
    {
      var id = Guid.NewGuid();
      while (!Messages.TryAdd(id, message)) { Thread.Sleep(100); };
    }

    private void RemoveMessage(Guid id)
    {
      while (!Messages.TryRemove(id, out SubscriptionMessage item)) { Thread.Sleep(100); };
    }

    //Public

    public void AddListner(Listener listener)
    {
      var id = Guid.NewGuid();
      while (!Listeners.TryAdd(id, listener)) { Thread.Sleep(100); };
      NewListeners = true;
    }

    public void RemoveListner(Guid id)
    {
      while (!Listeners.TryRemove(id, out Listener item)) { Thread.Sleep(100); };
    }

    public bool Publish(PublishMessage message)
    {
      try
      {
        Client.Publish(message.Topic, StringToBytes(message.Message), message.QosLevel, message.Retain);
        return true;
      }
      catch (Exception ex)
      {
        Logger.LogError("An Error Occured: {0} \r\nStack: {1}", ex.Message, ex.StackTrace);
        return false;
      }
    }

    public async Task<SubscriptionMessage> WaitForMessage(string topicRegex, TimeSpan timeout = default, bool doSee = true)
    {
      if (timeout.Ticks == 0)
        timeout = TimeSpan.FromSeconds(10);

      SubscriptionMessage message = null;

      AddListner(new Listener(topicRegex, (msg) =>
      {
        message = msg;
      }, true, doSee));

      return await Task.Run(async () =>
      {
        var start = DateTime.Now;
        while (message == null && (DateTime.Now - start) < timeout)
        {
          await Task.Delay(100);
        }
        return message;
      });
    }

    //UTIL

    private string BytesToString(byte[] val)
    {
      return Encoding.UTF8.GetString(val);
    }

    private byte[] StringToBytes(string val)
    {
      return Encoding.UTF8.GetBytes(val);
    }

  }

  // Classes
  public class Listener
  {
    public string TopicRegex { get; }
    public SubscribeCallback Callback { get; }
    public bool Once { get; }
    public bool DoSee { get; }


    public Listener(string topicRegex, SubscribeCallback callback, bool once = false, bool doSee = true)
    {
      TopicRegex = topicRegex;
      Callback = callback;
      Once = once;
      DoSee = doSee;
    }

    public bool MatchTopic(string actualTopic)
    {
      var pattern = TopicRegex
        .Replace("+", ".+")
        .Replace("..+", ".+")
        .Replace("#", ".+");

      return Regex.IsMatch(actualTopic, pattern);
    }

  }

  public class MQTTMessage
  {
    public string Topic { get; }
    public string Message { get; }
    public byte QosLevel { get; }
    public bool Retain { get; }

    public MQTTMessage(string topic, string msg, byte qosLevel, bool retain)
    {
      Topic = topic;
      Message = msg;
      QosLevel = qosLevel;
      Retain = retain;
    }

    public override string ToString()
    {
      return $"{Topic} : {Message}";
    }
  }

  public class PublishMessage : MQTTMessage
  {

    public PublishMessage(string topic, string msg, bool retain = false, byte qosLevel = 1) : base(topic, msg, qosLevel, retain)
    {
    }
  }

  public class SubscriptionMessage : MQTTMessage
  {
    public bool Seen { get; set; }
    public bool DupFlag { get; }
    public SubscriptionMessage(string topic, string msg, byte qosLevel, bool retain, bool dupFlag) : base(topic, msg, qosLevel, retain)
    {
      DupFlag = dupFlag;
      Seen = false;
    }
  }


}
