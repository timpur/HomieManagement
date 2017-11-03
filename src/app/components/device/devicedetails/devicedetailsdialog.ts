import { Component, Input } from '@angular/core';

import { HomieDevice } from "app/model/homiedevice";

@Component({
    selector: 'devicedetailsdialog',
    templateUrl: 'devicedetailsdialog.html',
    styleUrls: ["devicedetailsdialog.css"]
})
export class DeviceDetailsDialog {

    public Device: HomieDevice;

    constructor() {

    }
}
