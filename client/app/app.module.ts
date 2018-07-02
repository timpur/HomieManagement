import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { ApolloModule } from 'apollo-angular';
import { HttpLinkModule } from 'apollo-angular-link-http';

import { MaterialModule } from './material.module';
import { AppComponent } from './app.component';
import { GraphQLClientService } from '../services/GraphQLClientService';


@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    ApolloModule,
    HttpLinkModule,
    MaterialModule
  ],
  providers: [
    GraphQLClientService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
