import { Component } from '@angular/core';
import { GraphQLClientService } from '../services/GraphQLClientService';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'app';
  constructor(private client: GraphQLClientService) {
    this.client.query();
  }
}
