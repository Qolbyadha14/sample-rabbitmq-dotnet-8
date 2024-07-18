using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace subscriber.Services
{
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
}
