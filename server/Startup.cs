using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Server.Transports.WebSockets;
using GraphQL.Server.Ui.Playground;
using HomieManagement.MQTT;

namespace HomieManagement
{
    public class Startup
    {

        public IHostingEnvironment HostingEnvironment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env, IConfiguration config)
        {
            HostingEnvironment = env;
            Configuration = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Configuration>(Configuration.GetSection("Config"));
            services.AddResponseCompression();
            services.AddMvcCore();

            services.AddSingleton<QuerySchema>();
            services.AddGraphQLHttp();
            services.AddGraphQLWebSocket<QuerySchema>();

            // services.Configure<ExecutionOptions<TestAppSchema>>(options =>
            // {
            //     options.EnableMetrics = true;
            //     options.ExposeExceptions = true;
            //     options.UserContext = "something";
            // });

            services.AddSingleton<MQTTManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider provider)
        {
            app.UseResponseCompression();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();

            app.UseGraphQLHttp<QuerySchema>(new GraphQLHttpOptions());
            app.UseWebSockets();
            app.UseGraphQLWebSocket<QuerySchema>(new GraphQLWebSocketsOptions());
            app.UseGraphQLPlayground(new GraphQLPlaygroundOptions());

            var clientManage = provider.GetService<MQTTManager>();
            Task.Run(async () =>
            {
                await clientManage.Connect();
            });
        }
    }
}
