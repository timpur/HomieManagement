import { Component, OnInit } from '@angular/core';

import { HomieDevice } from "app/model/homiedevice";
import { Firmware } from "app/model/firmware";
import { HomieHubService } from "app/services/homiehub";

const FW_206_STATUS_REGEX = /[0-9]{3} ([0-9]+)\/([0-9]+)/g;

@Component({
  selector: 'app-updatedevice',
  templateUrl: './updatedevice.component.html',
  styleUrls: ['./updatedevice.component.css']
})
export class UpdateDeviceDialogComponent implements OnInit {

  public device: HomieDevice;
  public selectedFirmwareChain: Firmware;
  public firmware: Firmware;

  public get devices() {
    return this.homieHub.Devices.data;
  }

  public get firmwares() {
    let items: Array<Firmware> = [];
    this.homieHub.FirmwareList.data.forEach(item => {
      if (items.find(item2 => item2.Name == item.Name) == null) items.push(item);
    });
    return items;
  }

  public get firmwareChain() {
    let list = this.homieHub.FirmwareList.data
      .filter(item => {
        if (this.selectedFirmwareChain)
          return item.Name == this.selectedFirmwareChain.Name;
        else
          return false;
      })
      .sort((a, b) => {
        if (a.Version < b.Version)
          return -1
        if (a.Version > b.Version)
          return 1
        return 0
      });
    return list == null ? [] : list;
  }

  public updateStage: UpdateStages;
  public displayMSG: string;
  public wrBytes: number;
  public totalBytes: number;
  private watchID: number;

  constructor(private homieHub: HomieHubService) {
    this.updateStage = UpdateStages.NotStarted;
  }

  ngOnInit() {

  }

  public startUpdate() {
    let device = this.device
    let firmware = this.firmware;
    if (device && firmware) {
      this.homieHub.updateDeviceFirmware(device.HMDeviceID, firmware.ID)
        .then(res => {
          if (res.Success) {
            this.updateStage = UpdateStages.Updating;
            this.displayMSG = res.Message;
            this.startProgressWatcher();
          }
          else {
            this.updateStage = UpdateStages.Error;
            this.displayMSG = res.Message;
          }
        });
      this.updateStage = UpdateStages.Started;
    }
  }

  public startProgressWatcher() {
    this.watchID = window.setInterval(() => {
      if (this.updateStage == UpdateStages.Updating)
        this.getProgress();
      else
        window.clearInterval(this.watchID);
    }, 10);
  }

  public getProgress() {

    let status = this.device.OTAStatus;
    if (status == null) status = "";

    if (status.startsWith("200")) {
      this.wrBytes = this.totalBytes;
      this.updateStage = UpdateStages.Finished;
      // Done;
    }
    else if (status.startsWith("206")) {
      var match = FW_206_STATUS_REGEX.exec(status);
      if (match) {
        var wr = match[1];
        var total = match[2];
        if (wr && total) {
          this.wrBytes = Number(wr);
          this.totalBytes = Number(total);
        }
      }
    }
    //Impement all errors
    else if (status.startsWith("400")) {
      this.updateStage = UpdateStages.Error;
      this.displayMSG = status;
    }
    else if (status.startsWith("500")) {
      this.updateStage = UpdateStages.Error;
      this.displayMSG = status;
    }
  }

  public getProgressValue() {
    var val = this.wrBytes / this.totalBytes;
    if (isNaN(val)) val = 0;
    val *= 100;
    val = Math.round(val);
    return val;
  }

}

enum UpdateStages {
  NotStarted = 0,
  Started = 1,
  Updating = 2,
  Finished = 3,
  Error = -1
}
