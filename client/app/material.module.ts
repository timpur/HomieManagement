import { NgModule } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatButtonModule } from '@angular/material';


const imports = [
  BrowserAnimationsModule,
  MatButtonModule
];

@NgModule({
  imports: imports,
  exports: imports
})
export class MaterialModule { }
