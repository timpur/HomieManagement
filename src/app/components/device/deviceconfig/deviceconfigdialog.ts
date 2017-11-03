import { Component } from '@angular/core';

import { HomieDevice } from "app/model/homiedevice";

@Component({
  selector: 'deviceconfigdialog',
  templateUrl: 'deviceconfigdialog.html',
  styleUrls: ["deviceconfigdialog.css"]
})
export class DeviceConfigDialog {

  public Device: HomieDevice;
  public AddSetting: AddSetting;

  public Types: Array<string> = ["Text", "Number", "Boolean"];

  constructor() {
    this.AddSetting = new AddSetting();
  }

  public type(item) {
    return typeof item;
  }

  public Add(): void {
    switch (this.AddSetting.Type) {
      case "Text":
        this.Device.Config.Settings[this.AddSetting.Name] = "";
        break;
      case "Number":
        this.Device.Config.Settings[this.AddSetting.Name] = 0;
        break;
      case "Boolean":
        this.Device.Config.Settings[this.AddSetting.Name] = false;
        break;
    }
    this.AddSetting = new AddSetting();
  }
}
class AddSetting {
  public Name: string;
  public Type: string;
}
