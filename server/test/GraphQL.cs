using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using GraphQL.Conventions;
using GraphQL.Conventions.Relay;
using System.Reflection;
using GraphQL.Conventions.Types.Resolution;
using GraphQL.Conventions.Adapters;
using GraphQL.Conventions.Builders;
using System.Linq;

namespace HomieManagement
{
    public class QuerySchema : Schema
    {
        Schema _schema { get; }
        public QuerySchema()
        {
            _schema = CustomGraphQLEngine.New<Query>().GetSchema() as Schema;
            Query = _schema.Query;
            Mutation = _schema.Mutation;
            Subscription = _schema.Subscription;
            Directives = _schema.Directives;
            FieldNameConverter = _schema.FieldNameConverter;
            DependencyResolver = _schema.DependencyResolver;
        }
    }

    [ImplementViewer(OperationType.Query)]
    public class Query
    {

        [Description("Test Field")]
        public string Test { get; set; } = "Test Field Value";

        [Description("Get Users")]
        public User GetUser(int id, string value = "Example") => new User(id, value);
    }

    [Description("User Object")]
    public class User
    {
        [Description("User ID")]
        public int Id { get; }

        [Description("User Value")]
        public string Value { get; }

        public User(int id, string value)
        {
            Id = id;
            Value = value;
        }
    }
}

