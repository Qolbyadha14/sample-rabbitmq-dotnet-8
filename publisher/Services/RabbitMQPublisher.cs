using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace publisher.Services
{

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
}
