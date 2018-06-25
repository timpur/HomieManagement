using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;

namespace HomieManagement
{
    [Route("api/test")]
    [ApiController]
    public class TestApp : Controller
    {
        public static async Task Run()
        {

        }
        [HttpGet]
        [Route("test")]
        public string Test()
        {
            var schema = new Schema { Query = new TestAppQuery() };
            return schema.ToString();
        }
    }

    public class TestAppSchema : Schema
    {
        public TestAppSchema()
        {
            Query = new TestAppQuery();
        }
    }

    public class TestAppQuery : ObjectGraphType
    {
        public TestAppQuery()
        {
            Field<TestType>(
              name: "test",
              description: "Test des",
              arguments: new QueryArguments(new QueryArgument<IntGraphType> { Name = "param" }),
              resolve: context => new Test(context.GetArgument<int>("param"))
            );
            Field<TestType>(
              name: "test2",
              arguments: new QueryArguments(new QueryArgument<IntGraphType> { Name = "p" }),
              resolve: context => new Test(context.GetArgument<int>("p"))
            );
        }
    }

    public class TestType : ObjectGraphType<Test>
    {
        public TestType()
        {
            Field(x => x.TestField).Description("The test field");
        }
    }
    public class Test
    {
        public string TestField { get; set; }

        public Test(int param)
        {
            TestField = param.ToString();
        }
    }

}
