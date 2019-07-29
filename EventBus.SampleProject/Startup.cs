using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventBus.SampleProject.Configuration;
using EventBus.SampleProject.Events;
using Finaps.EventBus.AzureServiceBus;
using Finaps.EventBus.AzureServiceBus.DependencyInjection;
using Finaps.EventBus.Core.DependencyInjection;
using Finaps.EventBus.RabbitMQ;
using Finaps.EventBus.RabbitMQ.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventBus.SampleProject
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
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
      services.AddTransient<MessagePostedEventHandler>();
      services.AddTransient<MessagePutEventHandler>();
      EventBusConfiguration eventBusConfiguration = new EventBusConfiguration();
      Configuration.GetSection("EventBus").Bind(eventBusConfiguration);
      if (eventBusConfiguration.UseRabbitMQ)
      {
        var rabbitConfig = eventBusConfiguration.RabbitMQConfiguration;
        services.AddRabbitMQ(new RabbitMQOptions()
        {
          HostName = rabbitConfig.Host,
          UserName = rabbitConfig.UserName,
          Password = rabbitConfig.Password,
          VirtualHost = rabbitConfig.VirtualHost,
          QueueName = rabbitConfig.QueueName,
          ExchangeName = rabbitConfig.ExchangeName
        });
      }
      else
      {
        var azureConfig = eventBusConfiguration.AzureServiceBusConfiguration;
        services.AddAzureServiceBus(new AzureServiceBusOptions()
        {
          ClientName = azureConfig.ClientName,
          ConnectionString = azureConfig.ConnectionString
        });
      }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      //app.UseHttpsRedirection();
      app.UseMvc();
      app.AddEventHandler<MessagePostedEvent, MessagePostedEventHandler>();
      app.AddEventHandler<MessagePutEvent, MessagePutEventHandler>();
    }
  }
}
