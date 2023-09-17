# What is Fluent Queue?

The Fluent Queue component provides a unified API across a variety of different queue services for the dotnet framework.

## How can Install Fluent Queue?

```sh
dotnet add package Fluent.Queue --version 1.0.2
```

## How can use Fluent Queue?

### 1. Producing

**registration**

Register all your queue connections in program.cs and also assign a name to each connection, to able setup custom connection for queues. and set a ConnectionName as the default connection to use if the queue connection is not changed.

```c#
builder.Services.AddMessageBus(
    connections: new Dictionary<string, BaseBusConnectionDtoAbstract>
    {
        {
            "LocalRabbit",
            new RabbitMqConnectionDto{
                HostName = "127.0.0.1",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            }
        }
    },
    defaultConnection: "LocalRabbit"
);

```

**usage**

Inject `FluentQueue.Interfaces.Bus.IBus` into your class through `DI`. Then you can publish a message in the queue using the following code example.

> Messages must be inherit of `FluentQueue.Interfaces.Message.IMessage`

```c#
public abstract class MessageBase : IMessage
{
    public MessageBase(object body, IMessageProperties? properties = null)
    {
        Body = body;
        Properties = properties;
    }

    public object Body { get; set; }
    public IMessageProperties? Properties { get; set; }
}

public class TestMessageBody
{
    public required string Key { get; set; }
    public required string Value { get; set; }
}

public class TestProperties : IMessageProperties
{
    public string? CorrelationId { get; set; }
    public DateTime? Expiration { get; set; }
}

public class TestExchange : IExchange
{
    public string Name { get; set; } = "test";
    public string Type { get; set; } = "x-delayed-message";
}

public class TestMessage : MessageBase, IRabbitMqMessage
{
    public TestMessage(object body, IMessageProperties? properties = null) : base(body, properties)
    {
        Exchange = new TestExchange();
        RoutingKey = null;
    }

    public IExchange? Exchange { get; set; }
    public string? RoutingKey { get; set; }
}

```

> Queues must be inherit of `FluentQueue.Interfaces.Queue.IQueue`

```c#
public abstract class RabbitMqQueueBase : IRabbitMqQueue
{
    public abstract string QueueName { get; set; }
    public bool Durable { get; set; } = true;
    public bool Exclusive { get; set; } = false;
    public bool AutoDelete { get; set; } = false;
    public IDictionary<string, object>? Arguments { get; set; } = new Dictionary<string, object>();
}

public class TestQueue : RabbitMqQueueBase
{
    public override string QueueName { get; set; } = "test-queue";
}
```

> The use of `OnQueue`, `OnConnection` and `OnDelay` is optional.

> Warning ⚠️
> 
> If you don't send `OnQueue`, the package will generate a default queue.

```c#
_bus.Message(
    message: new TestMessage(
        body: new TestMessageBody
        {
            Key = "key",
            Value = "value"
        },
        properties: new TestProperties
        {
            CorrelationId = "test_correlation_id",
            // Expiration = DateTime.Now.AddDays(1)
        }
    )
).OnQueue(
    queue: new TestQueue()
).OnConnection(
    connection: "LocalRabbit"
).OnDelay(
    availableAt: DateTime.Now.AddSeconds(120)
).Dispatch();
```

### 2. Subscribing

**registration**

Similar to the first case, first make the connection of the queues. Then you can define your subscribers. Sending `connectionName`, `retry` and `consumerCount` is optional.

```c#
builder.Services.AddMessageBus(
    connections: new Dictionary<string, BaseBusConnectionDtoAbstract>
    {
        {
            "LocalRabbit",
            new RabbitMqConnectionDto{
                HostName = "127.0.0.1",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            }
        }
    },
    defaultConnection: "LocalRabbit"
).AddSubscriberBus(
    queues: new List<IQueue>{
        new TestQueue()
    },
    connectionName: "LocalRabbit",
    retry: 3,
    consumerCount: 1
);
```

**usage**

To use, you must define jobs that inherit from `FluentQueue.Interfaces.Job.IInvocableJob<TMessage>`. Whenever the message in the queue can be deserialized with the generic job parameter. Then this job will be fired.

```c#
public class TestJob : IInvocableJob<TestMessageBody>
{
    public Task FailedJob(TestMessageBody message, object? properties)
    {
        throw new NotImplementedException();
    }

    public Task Invoke(TestMessageBody message, string? correlationId)
    {
        throw new NotImplementedException();
    }
}

```
## What drivers does Fluent Queue support?

> More drivers will be added to following collection in the near future.

1. RabbitMQ

## How to add custom driver to Fluent Queue?

Custom driver registration has four steps:

1. Create a dto of connection parameters. With the help of inheritance from `FluentQueue.Implementation.Connection.BaseBusConnectionDtoAbstract`.
2. Create a connection. With the help of inheritance from `FluentQueue.Implementation.Connection.ConnectionBuilderAbstract<TConnection, TConnectionDto>`.
3. Create a producer. With the help of inheritance from `FluentQueue.Implementation.Bus.ProducerAbstract<TConnection, TMessage>`.
4. Create a consumer. With the help of inheritance from `FluentQueue.Implementation.Bus.SubscriberAbstract<TConnection>`.

> Note ⚠️
> 
> In the producer, if the queue is not sent, a default queue must be created.