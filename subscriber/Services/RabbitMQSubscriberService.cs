namespace subscriber.Services
{
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
}
