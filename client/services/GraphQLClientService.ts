import { Injectable } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { HttpLink } from 'apollo-angular-link-http';
import { InMemoryCache } from 'apollo-cache-inmemory';
import gql from 'graphql-tag';


@Injectable()
export class GraphQLClientService {
  constructor(private apollo: Apollo, httpLink: HttpLink) {
    apollo.create({
      link: httpLink.create({ uri: '/graphql' }),
      cache: new InMemoryCache(),
    });
  }

  query() {
    const query = gql`
      query {
        test
      }
    `;

    this.apollo.query({
      query
    })
      .subscribe((result) => console.log(result));

  }
}
