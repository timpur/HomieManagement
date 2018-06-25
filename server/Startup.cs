using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Server.Ui.Playground;

namespace HomieManagement
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore();

            services.AddSingleton<TestAppSchema>();
            services.AddGraphQLHttp();
            // services.Configure<ExecutionOptions<TestAppSchema>>(options =>
            // {
            //     options.EnableMetrics = true;
            //     options.ExposeExceptions = true;
            //     options.UserContext = "something";
            // });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseGraphQLHttp<TestAppSchema>(new GraphQLHttpOptions());
            app.UseGraphQLPlayground(new GraphQLPlaygroundOptions());

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
