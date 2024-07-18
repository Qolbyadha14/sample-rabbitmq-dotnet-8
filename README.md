```markdown
# RabbitMQ Publisher and Subscriber in .NET Core 8

This project demonstrates how to implement RabbitMQ publisher and subscriber using different types of exchanges (Direct, Fanout, Topic, and Headers) in .NET Core 8.

## Prerequisites

- [.NET Core 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [RabbitMQ](https://www.rabbitmq.com/download.html) server running locally or on a remote server

## Getting Started

### Clone the repository

```bash
git clone https://github.com/yourusername/rabbitmq-dotnet-core-example.git
cd rabbitmq-dotnet-core-example
```

### Install dependencies

Make sure you have the `RabbitMQ.Client` NuGet package installed:

```bash
dotnet add package RabbitMQ.Client
```

### Configuration

Configure RabbitMQ settings in `appsettings.json`:

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  }
}
```

### Publisher Implementation

The `RabbitMQPublisher` class is responsible for publishing messages to various types of exchanges.

```csharp
using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

public class RabbitMQPublisher
{
    private readonly IConfiguration _configuration;
    private IConnection _connection;
    private IModel _channel;

    public RabbitMQPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
        InitRabbitMQ();
    }

    private void InitRabbitMQ()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ:HostName"],
            UserName = _configuration["RabbitMQ:UserName"],
            Password = _configuration["RabbitMQ:Password"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Direct Exchange
        _channel.ExchangeDeclare(exchange: "direct-exchange", type: ExchangeType.Direct);
        _channel.QueueDeclare(queue: "direct-queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(queue: "direct-queue", exchange: "direct-exchange", routingKey: "direct-key");

        // Fanout Exchange
        _channel.ExchangeDeclare(exchange: "fanout-exchange", type: ExchangeType.Fanout);
        _channel.QueueDeclare(queue: "fanout-queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(queue: "fanout-queue", exchange: "fanout-exchange", routingKey: "");

        // Topic Exchange
        _channel.ExchangeDeclare(exchange: "topic-exchange", type: ExchangeType.Topic);
        _channel.QueueDeclare(queue: "topic-queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(queue: "topic-queue", exchange: "topic-exchange", routingKey: "topic.*");

        // Headers Exchange
        _channel.ExchangeDeclare(exchange: "headers-exchange", type: ExchangeType.Headers);
        _channel.QueueDeclare(queue: "headers-queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        var headers = new Dictionary<string, object> { { "x-match", "all" }, { "type", "log" }, { "level", "info" } };
        _channel.QueueBind(queue: "headers-queue", exchange: "headers-exchange", routingKey: "", arguments: headers);
    }

    public void PublishDirect(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "direct-exchange", routingKey: "direct-key", basicProperties: null, body: body);
        Console.WriteLine($"[Direct] Sent {message}");
    }

    public void PublishFanout(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "fanout-exchange", routingKey: "", basicProperties: null, body: body);
        Console.WriteLine($"[Fanout] Sent {message}");
    }

    public void PublishTopic(string message, string routingKey)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "topic-exchange", routingKey: routingKey, basicProperties: null, body: body);
        Console.WriteLine($"[Topic] Sent {message} with routing key {routingKey}");
    }

    public void PublishHeaders(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        var properties = _channel.CreateBasicProperties();
        properties.Headers = new Dictionary<string, object> { { "type", "log" }, { "level", "info" } };
        _channel.BasicPublish(exchange: "headers-exchange", routingKey: "", basicProperties: properties, body: body);
        Console.WriteLine($"[Headers] Sent {message} with headers");
    }

    ~RabbitMQPublisher()
    {
        _channel.Close();
        _connection.Close();
    }
}
```

### Subscriber Implementation

The `RabbitMQSubscriber` class is responsible for consuming messages from the queues.

```csharp
using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RabbitMQSubscriber
{
    private readonly IConfiguration _configuration;
    private IConnection _connection;
    private IModel _channel;

    public RabbitMQSubscriber(IConfiguration configuration)
    {
        _configuration = configuration;
        InitRabbitMQ();
    }

    private void InitRabbitMQ()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ:HostName"],
            UserName = _configuration["RabbitMQ:UserName"],
            Password = _configuration["RabbitMQ:Password"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Direct Exchange
        _channel.QueueDeclare(queue: "direct-queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        // Fanout Exchange
        _channel.QueueDeclare(queue: "fanout-queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        // Topic Exchange
        _channel.QueueDeclare(queue: "topic-queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        // Headers Exchange
        _channel.QueueDeclare(queue: "headers-queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void Subscribe()
    {
        SubscribeToQueue("direct-queue");
        SubscribeToQueue("fanout-queue");
        SubscribeToQueue("topic-queue");
        SubscribeToQueue("headers-queue");
    }

    private void SubscribeToQueue(string queueName)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"[Subscriber] Received message from {queueName}: {message}");
        };

        _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
    }

    ~RabbitMQSubscriber()
    {
        _channel.Close();
        _connection.Close();
    }
}
```

### Run the Subscriber as a Hosted Service

The `RabbitMQSubscriberService` class runs the subscriber when the application starts.

```csharp
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class RabbitMQSubscriberService : IHostedService
{
    private readonly RabbitMQSubscriber _rabbitMQSubscriber;

    public RabbitMQSubscriberService(RabbitMQSubscriber rabbitMQSubscriber)
    {
        _rabbitMQSubscriber = rabbitMQSubscriber;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _rabbitMQSubscriber.Subscribe();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

### Register Services in `Program.cs`

```csharp
using subscriber.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Project Publisher
builder.Services.AddSingleton<RabbitMQPublisher>();

//Project Subscriber
builder.Services.AddSingleton<RabbitMQSubscriber>();
builder.Services.AddHostedService<RabbitMQSubscriberService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

```

### API Endpoints

Use the following API endpoints to send messages to RabbitMQ:

- **Direct Exchange**
  ```http
  POST /rabbitmq/direct
  Body: "Your message here"
  ```

- **Fanout Exchange**
  ```http
  POST /rabbitmq/fanout
  Body: "Your message here"
  ```

- **Topic Exchange**
  ```http
  POST /rabbitmq/topic?routingKey=topic.key
  Body: "Your message here"
  ```

- **Headers Exchange**
  ```http
  POST /rabbitmq/headers
  Body: "Your message here"
  ```

### Running the Application

Run the application using the .NET CLI:

```bash
dotnet run
```
