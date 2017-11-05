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
    private IMqttClient Client { get; }
    CancellationToken ProccessingTaskToken { get; }
    private Task ProccessingTask { get; }
    private ConcurrentDictionary<Guid, SubscriptionMessage> Messages { get; }
    private ConcurrentDictionary<Guid, Listener> Listeners { get; }


    public MQTTManager(IOptions<AppConfig> config, ILogger<MQTTManager> logger)
    {
      Logger = logger;
      Config = config.Value;

      Messages = new ConcurrentDictionary<Guid, SubscriptionMessage>();
      Listeners = new ConcurrentDictionary<Guid, Listener>();

      // MQTT Setup
      Client = new MqttClientFactory().CreateMqttClient();
      Client.ApplicationMessageReceived += Client_ReceivedMQTTMessage;
      Client.Connected += async (s, e) =>
      {

        await Task.Delay(TimeSpan.FromSeconds(1));
        await Subscribe();
      };
      Client.Disconnected += async (s, e) =>
       {
         await Task.Delay(TimeSpan.FromSeconds(5));

         await Connect();
       };

      // ProccessingTask
      ProccessingTaskToken = new CancellationToken();
      ProccessingTask = Task.Factory.StartNew(async () =>
      {
        while (true)
        {
          ProccessingTaskToken.ThrowIfCancellationRequested();
          ProccessMessage();
          await Task.Delay(100);
        }
      }, ProccessingTaskToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    }

    public async Task<bool> Connect()
    {
      try
      {
        await Client.ConnectAsync(Config.MQTTConfig);
        return Client.IsConnected;
      }
      catch (Exception ex)
      {
        Logger.LogError("An Error Occured: {0} \r\nStack: {1}", ex.Message, ex.StackTrace);
      }
      return false;
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

    private async Task Subscribe()
    {
      var subs = Config.RootDeviceTopicLevels.Select(topic => new TopicFilter(topic + "#", MqttQualityOfServiceLevel.AtLeastOnce));
      await Client.SubscribeAsync(subs);
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
      return new MqttApplicationMessage(Topic, StringToBytes(Message), QosLevel, Retain);
    }

    private byte[] StringToBytes(string val)
    {
      return Encoding.UTF8.GetBytes(val);
    }
  }

  public class SubscriptionMessage : MQTTMessage
  {
    public bool Seen { get; set; }
    public SubscriptionMessage(string topic, string msg, MqttQualityOfServiceLevel qosLevel, bool retain) : base(topic, msg, qosLevel, retain)
    {
      Seen = false;
    }
  }


}
