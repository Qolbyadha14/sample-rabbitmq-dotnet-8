using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using publisher.Services;

namespace publisher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RabbitMQController : ControllerBase
    {
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public RabbitMQController(RabbitMQPublisher rabbitMQPublisher)
        {
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        [HttpPost("direct")]
        public IActionResult SendDirectMessage([FromBody] string message)
        {
            _rabbitMQPublisher.PublishDirect(message);
            return Ok("Direct message sent to RabbitMQ");
        }

        [HttpPost("fanout")]
        public IActionResult SendFanoutMessage([FromBody] string message)
        {
            _rabbitMQPublisher.PublishFanout(message);
            return Ok("Fanout message sent to RabbitMQ");
        }

        [HttpPost("topic")]
        public IActionResult SendTopicMessage([FromBody] string message, [FromQuery] string routingKey)
        {
            _rabbitMQPublisher.PublishTopic(message, routingKey);
            return Ok($"Topic message sent to RabbitMQ with routing key {routingKey}");
        }

        [HttpPost("headers")]
        public IActionResult SendHeadersMessage([FromBody] string message)
        {
            _rabbitMQPublisher.PublishHeaders(message);
            return Ok("Headers message sent to RabbitMQ");
        }
    }
}
