import { Component, Input } from '@angular/core';

import { HomieDevice } from "app/model/homiedevice";

@Component({
    selector: 'devicenodesdialog',
    templateUrl: 'devicenodesdialog.html',
    styleUrls: ["devicenodesdialog.css"]
})
export class DeviceNodesDialog {

    public Device: HomieDevice;

    constructor() {

    }
}
