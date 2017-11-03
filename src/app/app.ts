import { Component } from '@angular/core';

import { SharedService } from './services/shared';

@Component({
    selector: 'app-root',
    templateUrl: './app.html',
    styleUrls: ['./app.css']
})
export class AppComponent {

    get title(): string {
        return this.sharedService.title;
    }

    constructor(private sharedService: SharedService) {

    }

}
