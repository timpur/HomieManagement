import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-confirmationdialogue',
  templateUrl: './confirmationdialogue.component.html',
  styleUrls: ['./confirmationdialogue.component.css']
})

export class ConfirmationDialogueComponent implements OnInit {

    public title: string;
    public msgText: string;
    public posBtnText: string;
    public negBtnText: string;

    constructor() {}

    ngOnInit() {
    }

    public setAllText(title: string, msgText: string, posBtnText: string, negBtnText: string) {
        this.title = title;
        this.msgText = msgText;
        this.posBtnText = posBtnText;
        this.negBtnText = negBtnText;

    }

    public setText(title: string, msgText: string, posBtnText: string, negBtnText: string) {
        this.title = title;
        this.msgText = msgText;
        this.posBtnText = posBtnText;
        this.negBtnText = "Cancel";
    }



}
