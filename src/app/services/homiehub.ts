import { Injectable } from '@angular/core';
import { APP_BASE_HREF } from '@angular/common';
import { HubConnection } from '@aspnet/signalr-client';
import { TypedJSON, JsonObject, JsonMember } from '@upe/typedjson';
import { MatSnackBar } from '@angular/material';
import { ArrayDatabase } from "app/util/arraydatabase";

import { HomieDevicePage, HomieDevice, DeviceNode, NodeProperty } from '../model/homiedevice';
import { Firmware } from '../model/firmware';
import { Result } from "../model/result";

@Injectable()
export class HomieHubService {

  private homieHubConnection: HubConnection;

  public Devices: ArrayDatabase<HomieDevice>;
  public FirmwareList: ArrayDatabase<Firmware>;


  constructor(private snackBar: MatSnackBar) {
    this.Devices = new ArrayDatabase<HomieDevice>();
    this.FirmwareList = new ArrayDatabase<Firmware>();
    this.connect();
  }

  connect() {
    let base = "/";
    try {
      base = document.getElementsByTagName("base").item(0).attributes.getNamedItem("href").textContent;
    } catch (e) {
      this.notify("Error: Couldnt find base reff");
    }

    this.homieHubConnection = new HubConnection(`${base}signalr/homiehub`);

    // Listners
    this.onDeviceUpdate();
    this.onDeviceNodeUpdate();
    this.onDeviceNodePropertyUpdate();

    this.homieHubConnection.onclose((error) => {
      this.notify(`Homie Hub Dissconneced. Error: ${error.name} : ${error.message}`);
    });

    this.homieHubConnection.start().then(() => {
      this.getDevices();
      this.getFirmwareList();
    });
  }

  onDeviceUpdate() {
    this.homieHubConnection.on("DeviceUpdate", (data) => {
      let json: string = JSON.stringify(data);
      let result = TypedJSON.parse(json, HomieDevice) as HomieDevice;
      var device = this.Devices.find(device => device.HMDeviceID == result.HMDeviceID);
      if (device != null) {
        var cameOnline = device.Status == false && result.Status == true;
        device.update(result);
        if (cameOnline)
          this.notify(`${device.Name} (${device.Config.DeviceID}), came online`);
      }
    });
  }

  onDeviceNodeUpdate() {
    this.homieHubConnection.on("DeviceNodeUpdate", (deviceID: string, data) => {
      let json: string = JSON.stringify(data);
      let result = TypedJSON.parse(json, DeviceNode) as DeviceNode;
      var device = this.Devices.find(device => device.HMDeviceID == deviceID);
      if (device != null) {
        var node = device.Nodes.find(item => item.NodeID == result.NodeID)
        if (node != null) {
          node.update(result);
        }
      }
    });
  }

  onDeviceNodePropertyUpdate() {
    this.homieHubConnection.on("DeviceNodePropertyUpdate", (deviceID: string, nodeID: string, data) => {
      let json: string = JSON.stringify(data);
      let result = TypedJSON.parse(json, NodeProperty) as NodeProperty;
      var device = this.Devices.find(device => device.HMDeviceID == deviceID);
      if (device != null) {
        var node = device.Nodes.find(node => node.NodeID == nodeID)
        if (node != null) {
          var property = node.Properties.find(property => property.PropertyID == result.PropertyID)
          if (property != null) {
            property.update(result);
          }
        }
      }
    });
  }

  getDevices() {
    this.homieHubConnection.invoke("GetDevices")
      .then((result) => {
        let deviceList = result.map(obj => {
          let device = TypedJSON.parse(JSON.stringify(obj), HomieDevice) as HomieDevice;
          return device;
        }) as Array<HomieDevice>;
        this.Devices.setDB(deviceList);
        this.notify(`Recived ${this.Devices.data.length} devices`);
      });
  }

  sendDeviceConfigUpdate(device: HomieDevice) {
    this.homieHubConnection.invoke("DeviceConfigUpdate", device.HMDeviceID, TypedJSON.stringify(device.Config))
      .then(res => {
        return TypedJSON.parse(JSON.stringify(res), Result) as Result
      })
      .then((res) => {
        if (res.Success) {
          this.notify('Config Update was a Success');
        }
        else {
          this.notify(`Config Update was a Fail: ${res.Message}`);
        }
      });
  }

  sendDeviceReset(device: HomieDevice) {
    this.homieHubConnection.invoke("ResetDevice", device.HMDeviceID)
      .then(res => {
        return TypedJSON.parse(JSON.stringify(res), Result) as Result
      })
      .then((res) => {
        if (res.Success) {
          this.notify('Device Reset was a Success');
        }
        else {
          this.notify(`Device Reset was a Fail: ${res.Message}`);
        }
      });
  }

  getFirmwareList() {
    this.homieHubConnection.invoke("GetFirmware")
      .then((result) => {
        let firmwareList = result.map(obj => {
          let firmware = TypedJSON.parse(JSON.stringify(obj), Firmware) as Firmware;
          firmware.setDate(obj.date_created);
          return firmware;
        }) as Array<Firmware>;
        this.FirmwareList.addItems(firmwareList);
      });
  }

  addFrimware(firmwarestring: string, description: string) {
    this.homieHubConnection.invoke("AddFirmware", firmwarestring, description)
      .then(res => {
        return TypedJSON.parse(JSON.stringify(res), Result) as Result
      })
      .then((res) => {
        if (res.Success) {
          this.notify('Adding Firmware was a Success');
        }
        else {
          this.notify(`Adding Firmware was a Fail: ${res.Message}`);
        }
      });
  }

  removeFrimware(id: number) {
    this.homieHubConnection.invoke("RemoveFirmware", id)
      .then(res => {
        return TypedJSON.parse(JSON.stringify(res), Result) as Result
      })
      .then((res) => {
        if (res.Success) {
          this.notify('Removing Firmware was a Success');
        }
        else {
          this.notify(`Removing Firmware was a Fail: ${res.Message}`);
        }
      });
  }

  updateDeviceFirmware(DeviceID: string, FWID: number) {
    return this.homieHubConnection.invoke("UpdateDeviceFirmware", DeviceID, FWID)
      .then(res => {
        return TypedJSON.parse(JSON.stringify(res), Result) as Result
      })
      .then((res) => {
        if (res.Success) {
          this.notify(`Updateing Device Firmware was a Success: ${res.Message}`);
        }
        else {
          this.notify(`Updateing Device Firmware was a Fail: ${res.Message}`);
        }
        return res;
      });
  }


  notify(msg: string) {
    this.snackBar.open(msg, null, {
      duration: 5000
    });
  }
}
