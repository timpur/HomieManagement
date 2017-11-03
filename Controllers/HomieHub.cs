using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomieManagement.Model;
using HomieManagement.Model.DataBase;

namespace HomieManagement.Controllers
{
  public class HomieHub : Hub<IHomieClient>
  {
    private HomieDeviceManager HomieDeviceManager { get; }
    private FirmwareManager FirmwareManager { get; }

    public HomieHub(HomieDeviceManager deviceManager, FirmwareManager firmwareManager)
    {
      HomieDeviceManager = deviceManager;
      FirmwareManager = firmwareManager;
    }

    public IEnumerable<HomieDevice> GetDevices()
    {
      var list = HomieDeviceManager.GetValidDevices().OrderBy(item => item.MQTTRootTopicLevel);
      return list;
    }

    public Result DeviceConfigUpdate(Guid DeviceID, string DeviceConfigJSON)
    {
      var config = HomieDevice.DeviceConfig.FromJSON(DeviceConfigJSON);
      var success = HomieDeviceManager
          .FindDevice(DeviceID)
          .UpDateConfig(config);
      return new Result(success);
    }

    public Result ResetDevice(Guid DeviceID)
    {
      var success = HomieDeviceManager
          .FindDevice(DeviceID)
          .Reset();
      return new Result(success);
    }

    public IEnumerable<Firmware> GetFirmware()
    {
      var list = FirmwareManager.GetFirmwareList().OrderBy(item => item.Name).OrderBy(item => item.Version);
      return list;
    }

    public Result AddFirmware(string firmwareBase64, string description)
    {
      return FirmwareManager.AddFirmware(firmwareBase64, description);
    }

    public Result RemoveFirmware(int fwID)
    {
      FirmwareManager.RemoveFirmware(fwID);
      return new Result(true);
    }

    public Result UpdateDeviceFirmware(Guid deviceID, int fwID)
    {
      return FirmwareManager.UpdateDeviceFirmware(HomieDeviceManager.FindDevice(deviceID), FirmwareManager.FindFirmware(fwID));
    }
  }
}

namespace HomieManagement.Model
{
  // Client Interface
  public interface IHomieClient
  {
    void DeviceUpdate(HomieDevice device);
    void DeviceNodeUpdate(Guid deviceID, HomieDevice.DeviceNode node);
    void DeviceNodePropertyUpdate(Guid deviceID, string nodeID, HomieDevice.NodeProperty property);
  }

}
