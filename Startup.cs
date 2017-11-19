using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using HomieManagement.Controllers;
using HomieManagement.Model;
using HomieManagement.Model.DataBase;
using HomieManagement.Config;


namespace HomieManagement
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      // Add framework services.
      services.AddMvc()
          .AddJsonOptions(options =>
          {
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            options.SerializerSettings.ContractResolver = new DefaultContractResolver() { NamingStrategy = new DefaultNamingStrategy() };
          });

      services.AddSignalR(options =>
      {
        options.JsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
      });

      services.AddDbContext<HomieManagementContext>();

      // Services
      services.AddSingleton<MQTTManager>();
      services.AddSingleton<HomieDeviceManager>();
      services.AddSingleton<FirmwareManager>();
      services.AddTransient<HomieDevice>();

      // AppConfig Service
      services.Configure<AppConfig>(Configuration.GetSection("AppConfig"));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider, ILogger<Startup> logger)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseSignalR(routes =>
      {
        routes.MapHub<HomieHub>("signalr/homiehub");
      });

      app.UseMvc();

      app.Use(async (context, next) =>
      {
        await next();
        if (context.Response.StatusCode == 404 &&
                       !Path.HasExtension(context.Request.Path.Value) &&
                       !context.Request.Path.Value.StartsWith("/api/"))
        {
          context.Request.Path = "/index.html";
          await next();
        }
      });

      app.UseDefaultFiles();
      app.UseStaticFiles();

      // Start App
      try
      {
        serviceProvider.GetService<HomieManagementContext>().Initialize();
        serviceProvider.GetService<MQTTManager>().Connect().Wait();
      }
      catch (Exception ex)
      {
        logger.LogError("An Error Occured: {0} \r\nStack: {1}", ex.Message, ex.StackTrace);
      }
    }
  }
}
