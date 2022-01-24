# EventBus

The purpose of this package is to provide a clean and consistent interface for using a message broker in a system of ASP.NET Core web applications. Currently there are implementations targeting RabbitMQ and Azure Service Bus. Rather than exposing all the functionalities these messaging systems provide, this implementation uses just a small subset to provide a simple but robust pub/sub messaging setup.  
This package should be used in a ASP.NET Core application and uses the default Microsoft dependency injection (IServiceProvider).

## Setup

### RabbitMQ

To use RabbitMQ, add the following to your `Startup.cs`:

```csharp
var rabbitConfig = new RabbitMqOptions();
configuration.GetSection("Rabbit").Bind(rabbitConfig);

services.ConfigureRabbitMq(config =>
{
  config.Options = rabbitConfig;
  config.AddSubscription<MessagePostedEvent, MessagePostedEventHandler>();
});
```

Configuration options:

```csharp
  public class RabbitMqOptions
  {
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string HostName { get; set; } = "localhost";
    public int ConnectRetryCount { get; set; } = 10;
    public string ExchangeName { get; set; } = "amq.direct";
    public string QueueName { get; set; }
    public ushort PrefetchCount { get; set; } = 300;
  }
```

This implementation uses just one direct exchange for all applications interacting with each other, so make sure the provided exchange name is the same for all applications.
Similarly, each application has a single queue linked to this exchange for listening, so make sure the queue name is unique for each application.

### Azure Service Bus

To use Azure Service Bus, make sure you have created a topic in your service bus that all applications can publish and subscribe to, then create a subscription for each listening application. To connect your application, add the following code to your `Startup.cs`:

```csharp
  var azureConfig = configuration.GetSection(AzureServiceBusConfiguration.ConfigurationKey)
  .Get<AzureServiceBusConfiguration>();

  services.ConfigureAzureServiceBus(config =>
  {
    config.Options.ConnectionString = azureConfig.ConnectionString;
    config.Options.SubscriptionName = azureConfig.SubscriptionName;
    config.Options.TopicName = azureConfig.TopicName;
    config.AddSubscription<MessagePostedEvent, MessagePostedEventHandler>();
  });
```

Configuration options:

```csharp
  public class AzureServiceBusOptions
  {
    public string ConnectionString { get; set; }
    public string TopicName { get; set; }
    public string SubscriptionName { get; set; }
  }
```

## Events

To define an event, create a class that inherits from `IntegrationEvent`. For example, an event that some entity has been created could look as follows:

```csharp
public class EntityCreatedEvent : IntegrationEvent
{
  //List of properties detailing created entity
}
```

### Publishing

If you want to publish events from a certain class, make sure an instance of `IEventBus` is injected into the object. To publish, create an instance of a class derived from `IntegrationEvent` and call the `PublishAsync` method on the `IEventBus` instance. For example, to publish an event that an entity has been published from a controller:

```csharp
public EntityController : ControllerBase {
  private readonly IEventBus _eventBus;

  public EntityController(IEventBus eventBus)
  {
    _eventBus = eventBus;
  }

  [HttpPost]
  public async Task<ObjectResult> Create([FromBody] Entity entity)
  {
    //Entity creation logic
    //...

    //Create event object
    var event = new EntityCreatedEvent()
    {
      //Set properties appropriately
    };

    //Publish event
    _eventBus.PublishAsync(event);
    return Ok("sent");
  }
}
```

### Subscribing

To handle a certain event in your application, create a class inheriting from the `IIntegrationEventHandler<T>` interface and implement the `Handle` method. Continuing with our example of an event for an entity that has been created, the handler would look as follows:

```csharp
public class EntityCreatedEventHandler : IIntegrationEventHandler<EntityCreatedEvent>
{
  public async Task  Handle(EntityCreatedEvent @event)
  {
    // Logic for handling event
  }
}
```

Instances of your event handler are created as needed, when a message comes in. To make sure the Service Provider can construct the handler, make sure it is registered in your `Startup.cs`:

```csharp
services.AddTransient<EntityCreatedEventHandler>();
```

When constructing your Event Handler class, the service provider will also inject all its dependencies, so you can make use of your repositories, loggers etc. in your Handle method.

Finally, to make your application listen to events of a certain type, add a call to `AddSubscription` in the `Configure` method in your eventBus configuration:

```csharp
  services.ConfigureAzureServiceBus(config =>
  {
    //configure
    config.AddSubscription<MessagePostedEvent, MessagePostedEventHandler>(); //register event to handler
  });
```

## Do you have an issue?

Try to see if the issue persists within the included sample project. 

If you're still running into problems, feel free to file an issue.
