import { Component } from '@angular/core';
import { Observable } from "rxjs/Observable";

import { SharedService } from '../../../services/shared';
import { HomieHubService } from '../../../services/homiehub';
import { HomieDevice } from '../../../model/homiedevice';


@Component({
    selector: 'devicemanager',
    templateUrl: './devicemanager.html',
    styleUrls: ['./devicemanager.css']
})
export class DeviceManagerComponent {

    public deviceObserver: Observable<Array<HomieDevice>>;

    constructor(private sharedService: SharedService, private homieHub: HomieHubService) {
        this.sharedService.title = "Devices";
        this.deviceObserver = this.homieHub.Devices.getObservable();
    }
}
