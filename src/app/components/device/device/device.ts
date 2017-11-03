import { Component, Input } from '@angular/core';
import { MatDialog } from '@angular/material';

import { HomieDevice } from "app/model/homiedevice";
import { HomieHubService } from "app/services/homiehub";
import { DeviceDetailsDialog } from "../devicedetails/devicedetailsdialog";
import { DeviceConfigDialog } from "../deviceconfig/deviceconfigdialog";
import { DeviceNodesDialog } from "../devicenodes/devicenodesdialog";
import { ConfirmationDialogueComponent } from "app/util/confirmationdialogue/confirmationdialogue.component";


@Component({
  selector: 'device',
  templateUrl: './device.html',
  styleUrls: ['./device.css'],
  inputs: ["Device"]
})
export class DeviceComponent {

  @Input() public Device: HomieDevice;

  constructor(private dialog: MatDialog, private homieHub: HomieHubService) {

  }

  public viewDeviceDetails() {
    let dialogRef = this.dialog.open(DeviceDetailsDialog);
    let dialogInstance = dialogRef.componentInstance;
    dialogInstance.Device = this.Device;
  }

  public viewDeviceConfig() {
    let dialogRef = this.dialog.open(DeviceConfigDialog);
    let dialogInstance = dialogRef.componentInstance;
    dialogInstance.Device = this.Device;
    dialogRef.afterClosed()
      .map(res => res ? JSON.parse(res) : 0)
      .subscribe((res) => {
        if (res == 1) {
          this.homieHub.sendDeviceConfigUpdate(this.Device);
        }
        else if (res == 2) {
          this.confirmReset();
        }
      });
  }

  public viewDeviceNodes() {
    let dialogRef = this.dialog.open(DeviceNodesDialog);
    let deviceDialog = dialogRef.componentInstance;
    deviceDialog.Device = this.Device;
  }

  private confirmReset() {
    let dialogRef = this.dialog.open(ConfirmationDialogueComponent);
    let dialogInstance = dialogRef.componentInstance;
    dialogInstance.setText("Remove Firmware", "Are you sure?", "Yes", "No");
    dialogRef.afterClosed()
      .map(res => res ? JSON.parse(res) : false)
      .subscribe((res) => {
        if (res) {
          this.homieHub.sendDeviceReset(this.Device);
        }
      });
  }

}
