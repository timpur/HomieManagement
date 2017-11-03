import { Component } from '@angular/core';
import { MatDialog, Sort } from '@angular/material';
import { ArrayDataSource, PropertySort } from "app/util/arraydatabase";

import { SharedService } from '../../../services/shared';
import { HomieHubService } from '../../../services/homiehub';
import { HomieDevice } from '../../../model/homiedevice';
import { Firmware } from '../../../model/firmware';
import { AdddFirmwareDialog } from "app/components/firmware/add/addfirmwaredialog";
import { UpdateDeviceDialogComponent } from "app/components/firmware/updatedevice/updatedevice.component";
import { ConfirmationDialogueComponent } from "app/util/confirmationdialogue/confirmationdialogue.component";

@Component({
  selector: 'firmwaremanager',
  templateUrl: './firmwaremanager.html',
  styleUrls: ['./firmwaremanager.css']
})
export class FirmwareManagerComponent {
  public columns = ['Name', 'Version', 'Description', 'DateCreated', 'Menu'];
  public dataSource: ArrayDataSource<Firmware>;
  public property: PropertySort = new PropertySort();

  constructor(private sharedService: SharedService, private homieHub: HomieHubService, private dialog: MatDialog, ) {
    this.sharedService.title = "Firmware";
    this.dataSource = new ArrayDataSource(this.homieHub.FirmwareList);
    this.dataSource.setPropertySort(this.property);
  }

  public sortData(sort: Sort): void {
    this.property.property = sort.active;
    this.property.direction = sort.direction == "desc" ? false : true;
    this.dataSource.refresh();
  }

  public showAddFirmwareDialog(): void {
    let dialogRef = this.dialog.open(AdddFirmwareDialog);
    let dialogInstance = dialogRef.componentInstance;
    dialogRef.afterClosed()
      .map(res => res ? JSON.parse(res) : false)
      .subscribe((res) => {
        if (res) {
          dialogInstance.readFile().then((result) => {
            this.homieHub.addFrimware(result, dialogInstance.description);
          }).catch((error) => {
            this.sharedService.showNotification(error);
          });
        }
      });
  }

  public showUpdateDeviceDialog(): void {
    let dialogRef = this.dialog.open(UpdateDeviceDialogComponent);
    let dialogInstance = dialogRef.componentInstance;
    dialogRef.afterClosed()
      .map(res => res ? JSON.parse(res) : false)
      .subscribe((res) => {

      });
  }

  public remove(item: Firmware) {
    let dialogRef = this.dialog.open(ConfirmationDialogueComponent);
    let dialogInstance = dialogRef.componentInstance;
    dialogInstance.setText("Remove Firmware", "Are you sure?", "Yes", "No");
    dialogRef.afterClosed()
      .map(res => res ? JSON.parse(res) : false)
      .subscribe((res) => {
        if (res) {
          this.homieHub.removeFrimware(item.ID);
        }
      });
  }


}
