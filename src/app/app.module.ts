// Core
import { BrowserModule } from '@angular/platform-browser';
import { NgModule, LOCALE_ID } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

// Material
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {
  MatButtonModule,
  MatCheckboxModule,
  MatCardModule,
  MatChipsModule,
  MatDialogModule,
  MatProgressBarModule,
  MatProgressSpinnerModule,
  MatIconModule,
  MatTableModule,
  MatSelectModule,
  MatSnackBarModule,
  MatSidenavModule,
  MatInputModule,
  MatFormFieldModule,
  MatListModule,
  MatToolbarModule,
  MatTooltipModule,
  MatStepperModule
} from '@angular/material';
import { FlexLayoutModule } from '@angular/flex-layout';

const MatImports = [
  BrowserAnimationsModule,
  FlexLayoutModule,
  MatButtonModule,
  MatCheckboxModule,
  MatCardModule,
  MatChipsModule,
  MatDialogModule,
  MatProgressBarModule,
  MatProgressSpinnerModule,
  MatIconModule,
  MatTableModule,
  MatSelectModule,
  MatSnackBarModule,
  MatSidenavModule,
  MatInputModule,
  MatFormFieldModule,
  MatListModule,
  MatToolbarModule,
  MatTooltipModule,
  MatStepperModule
];

// APP
import { AppComponent } from './app';
import { DeviceManagerComponent } from './components/device/manager/devicemanager';
import { DeviceComponent } from "app/components/device/device/device";
import { DeviceDetailsDialog } from "./components/device/devicedetails/devicedetailsdialog";
import { DeviceConfigDialog } from "./components/device/deviceconfig/deviceconfigdialog";
import { DeviceNodesDialog } from "./components/device/devicenodes/devicenodesdialog";
import { FirmwareManagerComponent } from './components/firmware/manager/firmwaremanager';
import { AdddFirmwareDialog } from './components/firmware/add/addfirmwaredialog';
import { ConfirmationDialogueComponent } from "./util/confirmationdialogue/confirmationdialogue.component";
import { UpdateDeviceDialogComponent } from './components/firmware/updatedevice/updatedevice.component';

import { SharedService } from './services/shared';
import { HomieHubService } from './services/homiehub';


@NgModule({
  imports: [
    BrowserModule,
    FormsModule,
    MatImports,
    RouterModule.forRoot([
      { path: '', redirectTo: '/devices', pathMatch: 'full' },
      { path: "devices", component: DeviceManagerComponent },
      { path: "firmware", component: FirmwareManagerComponent }
    ])
  ],
  declarations: [
    AppComponent,
    DeviceManagerComponent,
    DeviceComponent,
    DeviceDetailsDialog,
    DeviceConfigDialog,
    DeviceNodesDialog,
    FirmwareManagerComponent,
    AdddFirmwareDialog,
    ConfirmationDialogueComponent,
    UpdateDeviceDialogComponent
  ],
  providers: [
    SharedService,
    HomieHubService,
    { provide: LOCALE_ID, useValue: 'en-au' }
  ],
  entryComponents: [
    DeviceDetailsDialog,
    DeviceConfigDialog,
    DeviceNodesDialog,
    AdddFirmwareDialog,
    ConfirmationDialogueComponent,
    UpdateDeviceDialogComponent
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }


