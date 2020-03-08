# CodingCat.RabbitMq.Abstractions

Abstractions based on https://github.com/CodingCatHongKong/dotnet-CodingCat.Mq.Abstractions and https://github.com/rabbitmq/rabbitmq-dotnet-client


### Connect RabbitMq

As usually, we tend to provide the ability to connect to an external service with a retry extension

```csharp
// -- ConnectionFactory could be any class implementing https://github.com/rabbitmq/rabbitmq-dotnet-client/blob/master/projects/client/RabbitMQ.Client/src/client/api/IConnectionFactory.cs
// -- ConnectConfiguration could be any class implementing https://github.com/CodingCatHongKong/dotnet-CodingCat.Mq.Abstractions/blob/master/CodingCat.Mq.Abstractions/CodingCat.Mq.Abstractions/Interfaces/IConnectConfiguration.cs

new ConnectionFactory()
{
    Uri = new Uri("amqp://127.0.0.1")
}.CreateConnection(new ConnectConfiguration()
{
    TimeoutPerTry = TimeSpan.FromSeconds(30),
    RetryInterval = TimeSpan.FromSeconds(3),
    RetryUpTo = 3
});
```


### Initializing Exchanges and Queues

Using the `BaseInitializer`, it will automatically declare all the exchanges and queues when constructing. It also provide an abstract function `Configure` to setup the binding logic and a `GetSubscribed` function to kick start all the subscriber factories for the subscribed subscribers.

```csharp
public class Workers : BackgroundServices // -- these logic are provided in the BaseHostFactory
{
    public IInitializer RabbitMqInitializer { get; }
    
    //...
    
    protected override Task ExecuteAsync(
        CancellationToken stoppingToken
    )
    {
        return Task.Run(() =>
        {
            var subscribers = this.RabbitMqInitializer
                .Configure(this.Connection.CreateModel())
                .GetSubscribed(this.Connection);

            Console.WriteLine(new StringBuilder()
                .Append($"Subscribed {subscribers.Count()} ")
                .Append("subscriber(s)")
                .ToString()
            );

            stoppingToken.Register(() => Dispose(subscribers));
        });
    } 
    
    //...
}
```


### Setup the Subscriber

The subscriber (both `Subscriber<T>` & `Subscriber<TIn, TOut>`) will only take care converting the `byte[]` message to a concrete instance, and passing the coverted input to the `IProcessor`. There are several base processors are provided in https://github.com/CodingCatHongKong/dotnet-CodingCat.Mq.Abstractions/tree/master/CodingCat.Mq.Abstractions/CodingCat.Mq.Abstractions

The mentioned abstract processors already provide some powerful features such as process timeout, default output and some error handling, you could implement the `IProcessor` yourself if necessary.

```csharp
public class MessageSubscriber : BaseSubscriber<string>
{
    public MessageSubscriber(IModel channel, string queueName, IProcess<string> processor)
        : base(channel, queueName, processor)
    {
    }
    
    protected override TInput FromBytes(byte[] bytes)
    {
        return new StringSerializer().FromBytes(bytes); // -- https://github.com/CodingCatHongKong/dotnet-CodingCat.Serializers
    }
}
```


### Setup the SubscriberFactory

Instead of making the subscriber class dirty, this library provides the `ISubscriberFactory` to do the dirty setup. The subscriber factory is intended to prepare, setup, configure, initialize and make the subscriber to subscribe to a queue.

```csharp
public class MessageSubscriberFactory : ISubscriberFactory
{
    public MessageSubscriberFactory(
        IConnection connection,
        MessageExchange exchange,
        MessageQueue queue,
        MessageProcessor processor,
        IStorageManager storageManager
    )
    {
        //....
    }
    
    public ISubscriber GetSubscribed(IModel channel)
    {
        return new .....;
    }
}
```


### Setup the Publisher

A publisher is used to send messages to RabbitMq.

```csharp
public class MessagePublisher : BasePublisher<string>
{
    public MessagePublisher(IModel channel) : base(channel)
    {
        this.ExchangeName = Constants.DIRECT_EXCHANGE_NAME; // -- optional if direct push
        this.RoutingKey = Constants.MESSAGE_ROUTING_KEY;    // -- optional if the exchange will take care
    }
    
    protected override byte[] ToBytes(string input)
    {
        return new StringSerializer().ToBytes(input); // -- https://github.com/CodingCatHongKong/dotnet-CodingCat.Serializers
    }
}
```


### Setup the Publisher with output

There are some more options when setting up a publisher expecting an output.

```csharp
public MessagePublisher : BasePublisher<string, bool>
{
    public MessagePublisher(IConnection connection) : base(connection)
    {
        this.DefaultOutput = false;
        this.Timeout = TimeSpan.FromSeconds(30);
    }
    
    protected override byte[] ToBytes(string input) { ... }
    protected override bool FromBytes(byte[] bytes) { ... }
}
```