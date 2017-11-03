import { Injectable } from '@angular/core';
import { MatSnackBar } from "@angular/material";

@Injectable()
export class SharedService {

    public title: string;

    constructor(public snackBar: MatSnackBar) {
        this.title = "Homie Managemant";
    }

    showNotification(msg: string) {
        this.snackBar.open(msg, null, {
            duration: 5000
        });
    }

}
