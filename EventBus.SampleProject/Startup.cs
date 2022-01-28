using EventBus.SampleProject.Configuration;
using EventBus.SampleProject.Events;
using EventBus.SampleProject.Infrastructure.EventBus;
using Finaps.EventBus.RabbitMq.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
      services.AddControllers(options => options.EnableEndpointRouting = false);
      services.AddTransient<MessagePostedEventHandler>();
      services.AddTransient<MessagePutEventHandler>();
      services.AddTransient<KafkaMessagePostedEventHandler>();
      services.AddScoped<ScopedDependency>();
      EventBusConfiguration eventBusConfiguration = new EventBusConfiguration();
      Configuration.GetSection("EventBus").Bind(eventBusConfiguration);
      if (eventBusConfiguration.UseRabbitMQ)
      {
        services.ConfigureRabbitMq(Configuration);
      }
      else if(eventBusConfiguration.UseKafka)
      {
        services.ConfigureKafka(Configuration);
      }
      else
      {
        services.ConfigureAzureServiceBus(Configuration);
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
    }
  }
}
