using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Core;
using MQTTnet.Core.Client;
using MQTTnet.Core.ManagedClient;
using MQTTnet.Core.Packets;
using MQTTnet.Core.Protocol;
using HomieManagement.Config;


namespace HomieManagement.Model
{
  public delegate void onMQTTMessage(MQTTMessage topic);
  public delegate void SubscribeCallback(SubscriptionMessage message);

  public class MQTTManager
  {
    private ILogger Logger { get; }
    private AppConfig Config { get; }
    private IManagedMqttClient Client { get; }
    private Timer ProccessTimer { get; set; }
    private ConcurrentDictionary<Guid, SubscriptionMessage> Messages { get; }
    private ConcurrentDictionary<Guid, Listener> Listeners { get; }


    public MQTTManager(IOptions<AppConfig> config, ILogger<MQTTManager> logger)
    {
      Logger = logger;
      Config = config.Value;

      Messages = new ConcurrentDictionary<Guid, SubscriptionMessage>();
      Listeners = new ConcurrentDictionary<Guid, Listener>();

      // MQTT Setup
      Client = new MqttFactory().CreateManagedMqttClient();
      Client.ApplicationMessageReceived += Client_ReceivedMQTTMessage;
      Client.SubscribeAsync(Config.RootDeviceTopicLevels.Select(topic => new TopicFilter(topic + "#", MqttQualityOfServiceLevel.AtLeastOnce))).Wait();
      Client.Connected += (s, e) =>
      {
        Logger.LogInformation("Client Connected");
      };
      Client.Disconnected += (s, e) =>
      {
        Logger.LogInformation("Client Disconnected");
      };
    }

    public void Start()
    {
      Logger.LogInformation("Started MQTT Manager");

      // Start client
      Client.StartAsync(new ManagedMqttClientOptions
      {
        ClientOptions = Config.MQTTConfig.Options(),
        AutoReconnectDelay = TimeSpan.FromSeconds(5)
      }).Wait();
      // Start Proccess Task
      ProccessTimer = new Timer(ProccessTask, null, 500, 500);
    }

    private void Client_ReceivedMQTTMessage(object sender, MqttApplicationMessageReceivedEventArgs e)
    {
      try
      {
        AddMessage(new SubscriptionMessage(e.ApplicationMessage.Topic, BytesToString(e.ApplicationMessage.Payload), e.ApplicationMessage.QualityOfServiceLevel, e.ApplicationMessage.Retain));
      }
      catch (Exception ex)
      {
        Logger.LogError("An Error Occured: {0} \r\nStack: {1}", ex.Message, ex.StackTrace);
      }
    }

    private void ProccessTask(object state)
    {
      try
      {
        ProccessMessage();
      }
      catch (Exception ex)
      {
        Logger.LogError("An Error Occured: {0} \r\nStack: {1}", ex.Message, ex.StackTrace);
      }
    }

    private void ProccessMessage()
    {
      try
      {
        var messages = Messages.Where(item => !item.Value.Seen).OrderBy(item => item.Value.Recived).ToList();
        foreach (var message in messages)
        {
          ProccessMessageListerner(message.Value);
          if (message.Value.Seen || message.Value.Count > 50)
            RemoveMessage(message.Key);
        }
      }
      catch (Exception ex)
      {
        Logger.LogError("An Error Occured: {0} \r\nStack: {1}", ex.Message, ex.StackTrace);
      }
    }

    private void ProccessMessageListerner(SubscriptionMessage message)
    {
      var toRemoveListeners = new List<Guid>();

      var listeners = Listeners.Where(item => item.Value.MatchTopic(message.Topic));
      foreach (var listener in listeners)
      {
        listener.Value.Callback(message);
        if (listener.Value.Once) toRemoveListeners.Add(listener.Key);
      }

      toRemoveListeners.ForEach(item => RemoveListner(item));
    }

    private void AddMessage(SubscriptionMessage message)
    {
      //Logger.LogInformation($"New MQTT Message: {message.ToString()}");
      var id = Guid.NewGuid();
      while (!Messages.TryAdd(id, message))
      {
        Thread.Sleep(1);
      };
    }

    private void RemoveMessage(Guid id)
    {
      while (Messages.ContainsKey(id) && !Messages.TryRemove(id, out SubscriptionMessage item))
      {
        Thread.Sleep(1);
      };
    }

    //Public

    public Guid AddListner(Listener listener)
    {
      var id = Guid.NewGuid();
      while (!Listeners.TryAdd(id, listener)) { Thread.Sleep(100); };
      return id;
    }

    public void RemoveListner(Guid id)
    {
      while (!Listeners.TryRemove(id, out Listener item)) { Thread.Sleep(100); };
    }

    public async Task<bool> Publish(PublishMessage message)
    {
      try
      {
        await Client.PublishAsync(message.ToMQTTMessage());
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
          await Task.Delay(10);
        }
        return message;
      });
    }
    public async Task<SubscriptionMessage> WaitForMessage(string topicRegex, string messageRegex, TimeSpan timeout = default, bool doSee = true)
    {
      if (timeout.Ticks == 0)
        timeout = TimeSpan.FromSeconds(10);

      SubscriptionMessage message = null;
      bool found = false;

      AddListner(new Listener(topicRegex, (msg) =>
      {
        message = msg;
        found = Regex.IsMatch(message?.Message, messageRegex);
      }, true, doSee));

      return await Task.Run(async () =>
      {
        var start = DateTime.Now;
        while (message == null && !found && (DateTime.Now - start) < timeout)
        {
          await Task.Delay(10);
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
    public MqttQualityOfServiceLevel QosLevel { get; }
    public bool Retain { get; }

    public MQTTMessage(string topic, string msg, MqttQualityOfServiceLevel qosLevel, bool retain)
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

    public PublishMessage(string topic, string msg, bool retain = false, MqttQualityOfServiceLevel qosLevel = MqttQualityOfServiceLevel.AtLeastOnce) : base(topic, msg, qosLevel, retain)
    {
    }

    public MqttApplicationMessage ToMQTTMessage()
    {
      return new MqttApplicationMessageBuilder().WithTopic(Topic).WithPayload(Message).WithQualityOfServiceLevel(QosLevel).WithRetainFlag(Retain).Build();
    }

    private byte[] StringToBytes(string val)
    {
      return Encoding.UTF8.GetBytes(val);
    }
  }

  public class SubscriptionMessage : MQTTMessage
  {
    public bool Seen { get; set; }

    protected byte count;
    public byte Count { get { return ++count; } }

    public DateTime Recived { get; }
    public SubscriptionMessage(string topic, string msg, MqttQualityOfServiceLevel qosLevel, bool retain) : base(topic, msg, qosLevel, retain)
    {
      Seen = false;
      count = 0;
      Recived = DateTime.Now;
    }
  }


}
