using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;


namespace HomieManagement.Model
{
  public class HomieDeviceManager
  {
    private ILogger Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private MQTTManager MQTTManager { get; }

    public List<HomieDevice> Devices { get; }
    private Listener Listener { get; }

    public HomieDeviceManager(MQTTManager mqttManger, ILogger<HomieDeviceManager> logger, IServiceProvider serviceProvider)
    {
      Logger = logger;
      ServiceProvider = serviceProvider;
      MQTTManager = mqttManger;

      Devices = new List<HomieDevice>();

      Listener = new Listener(".+/\\$homie", DeviceTopicHandler);
      MQTTManager.AddListner(Listener);
    }

    public void DeviceTopicHandler(SubscriptionMessage message)
    {
      var deviceRootTopic = message.Topic.Replace("/$homie", "");
      if (Devices.Find(device => device.MQTTRootTopicLevel == deviceRootTopic) == null)
      {
        var device = ServiceProvider.GetService<HomieDevice>();
        device.Setup(deviceRootTopic);
        Devices.Add(device);
      }
    }

    public HomieDevice FindDevice(Guid id)
    {
      return Devices.First(x => x.HMDeviceID == id);
    }

    public List<HomieDevice> GetValidDevices()
    {
      return Devices.FindAll(x => x.Config != null);
    }

  }
}
