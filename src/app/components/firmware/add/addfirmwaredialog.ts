import { Component, ElementRef, ViewChild } from "@angular/core";

@Component({
  selector: 'addfirmwaredialog',
  templateUrl: 'addfirmwaredialog.html',
  styleUrls: ["addfirmwaredialog.css"]
})
export class AdddFirmwareDialog {

  @ViewChild("file") file: ElementRef;

  public description: string = "";

  constructor() {
  }

  public readFile(): Promise<string> {
    let promise = new Promise<string>((resolve, reject) => {
      let files = this.file.nativeElement.files as File[];
      if (files.length > 0) {
        let file = files[0];
        if (file.name.endsWith(".bin")) {
          let fileReader = new FileReader();
          fileReader.onload = function (scope) {
            return (event: Event) => {
              let fileString: string = (event.target as any).result;
              let fileBase64 = fileString.split(",")[1];
              scope.resolve(fileBase64);
            }
          }({ file: file, resolve: resolve });
          fileReader.readAsDataURL(file);
        }
        else
          reject("Wrong File Type, Use .bin");
      }
      else
        reject("No File Selected");
    });
    return promise;
  }
}
