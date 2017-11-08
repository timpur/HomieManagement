using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomieManagement.Model.DataBase;
using System.Text;

namespace HomieManagement.Model
{
  public class FirmwareManager
  {

    private static readonly byte[] HomieESP8266Signature = { 0x25, 0x48, 0x4F, 0x4D, 0x49, 0x45, 0x5F, 0x45, 0x53, 0x50, 0x38, 0x32, 0x36, 0x36, 0x5F, 0x46, 0x57, 0x25 };
    private static readonly byte[] HomieESP8266NameStart = { 0xBF, 0x84, 0xE4, 0x13, 0x54 };
    private static readonly byte[] HomieESP8266NameEnd = { 0x93, 0x44, 0x6B, 0xA7, 0x75 };
    private static readonly byte[] HomieESP8266VersionStart = { 0x6A, 0x3F, 0x3E, 0x0E, 0xE1 };
    private static readonly byte[] HomieESP8266VersionEnd = { 0xB0, 0x30, 0x48, 0xD4, 0x1A };

    public HomieManagementContext HomieManagement { get; }

    public FirmwareManager()
    {
      HomieManagement = new HomieManagementContext();
    }

    public void OnFirmwareChanged(Firmware firmware)
    {

    }

    public List<Firmware> GetFirmwareList()
    {
      return HomieManagement.FirmwarList.ToList();
    }

    public Firmware FindFirmware(int id)
    {
      return HomieManagement.FindFirmware(id);
    }

    public void RemoveFirmware(int id)
    {
      HomieManagement.RemoveFirmware(id);
    }


    public Result AddFirmware(string base64, string description)
    {
      var firmware = new Firmware()
      {
        Binary = Convert.FromBase64String(base64),
        Description = description,
        DateCreated = DateTime.Now.ToUniversalTime()
      };

      if (!ValidFirmware(firmware))
        return new Result(false, "Firmware is not Valid");

      if (HomieManagement.FirmwareExists(firmware))
        return new Result(false, "Firmware already Exists");

      HomieManagement.AddFirmware(firmware);

      OnFirmwareChanged(firmware);

      return new Result(true);

    }

    private bool ValidFirmware(Firmware firmware)
    {
      if (ValidHomieESP8266Firmware(firmware))
        return true;
      else
        return false;
    }

    // Homie ESP8266 Firmware Validator
    private bool ValidHomieESP8266Firmware(Firmware firmware)
    {
      if (IndexOf(firmware.Binary, HomieESP8266Signature) != -1)
      {
        var successName = SetHomieESP8266FirmwareName(firmware);
        var successVersion = SetHomieESP8266FirmwareVersion(firmware);
        if (successName && successVersion)
        {
          firmware.Type = FirmwareType.Homie_ESP8266;
          return true;
        }
      }
      return false;
    }
    private bool SetHomieESP8266FirmwareName(Firmware firmware)
    {
      var start = IndexOf(firmware.Binary, HomieESP8266NameStart) + HomieESP8266NameStart.Length;
      var end = IndexOf(firmware.Binary, HomieESP8266NameEnd);
      if (start != -1 && end != -1)
      {
        var nameBytes = firmware.Binary.Skip(start).Take(end - start).ToArray();
        var name = Encoding.ASCII.GetString(nameBytes);
        firmware.Name = name;
        return true;
      }
      return false;
    }
    private bool SetHomieESP8266FirmwareVersion(Firmware firmware)
    {
      var start = IndexOf(firmware.Binary, HomieESP8266VersionStart) + HomieESP8266VersionStart.Length;
      var end = IndexOf(firmware.Binary, HomieESP8266VersionEnd);
      if (start != -1 && end != -1)
      {
        var versionBytes = firmware.Binary.Skip(start).Take(end - start).ToArray();
        var version = Encoding.ASCII.GetString(versionBytes);
        firmware.Version = version;
        return true;
      }
      return false;
    }


    public Result UpdateDeviceFirmware(HomieDevice device, Firmware firmware)
    {
      //if (device.FWVersion == firmware.Version)
      //  return new Result(false, "The device is already on this version.");

      var checksum = firmware.GetHashHex();
      var firmwareStr = firmware.Base64Firmware();

      var result = device.SendFirmware(checksum, firmwareStr);
      var code = result.code;

      if (result.sent)
      {

        if (code.StartsWith("202"))
        {
          return new Result(true, "Update was accepted, flashing");
        }
        if (code.StartsWith("206"))
        {
          return new Result(true, "Missed start sequence, Update inprogress allready.");
        }
        else if (code.StartsWith("304"))
        {
          return new Result(false, "The device is already on this version");
        }
        else if (code.StartsWith("400"))
        {
          return new Result(false, $"An Error occueed with the sent firmware: {code}");
        }
        else if (code.StartsWith("403"))
        {
          return new Result(false, "OTA is not enabled");
        }
        else if (code.StartsWith("500 "))
        {
          return new Result(false, $"An Error occured on the device: {code}");
        }
        else
          return new Result(false, code);
      }

      return new Result(false, $"Something went wrong while updateing firmware: {code}");
    }


    // UTIL
    public static int IndexOf(byte[] arrayToSearchThrough, byte[] patternToFind)
    {
      if (patternToFind.Length > arrayToSearchThrough.Length)
        return -1;
      for (int i = 0; i < arrayToSearchThrough.Length - patternToFind.Length; i++)
      {
        bool found = true;
        for (int j = 0; j < patternToFind.Length; j++)
        {
          if (arrayToSearchThrough[i + j] != patternToFind[j])
          {
            found = false;
            break;
          }
        }
        if (found)
        {
          return i;
        }
      }
      return -1;
    }
  }
}
