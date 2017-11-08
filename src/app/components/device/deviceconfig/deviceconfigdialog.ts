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

  public SettingTypes: Map<string, string>;

  public Types: Array<string> = ["Text", "Number", "Boolean"];

  constructor() {
    this.AddSetting = new AddSetting();
    this.SettingTypes = new Map();
  }

  public settingType(settingName: string) {
    if (!this.SettingTypes.has(settingName))
      this.SettingTypes.set(settingName, typeof this.Device.Config.Settings[settingName]);
    return this.SettingTypes.get(settingName);
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

  public Remove(settingName) {
    delete this.Device.Config.Settings[settingName];
    this.SettingTypes.delete(settingName);
  }
}
class AddSetting {
  public Name: string;
  public Type: string;
}
