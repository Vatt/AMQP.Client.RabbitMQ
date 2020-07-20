namespace AMQP.Client.RabbitMQ.Protocol.Exceptions
{
    public class RabbitMQConsumeOkTagMissmatchException : RabbitMQException
    {
        public RabbitMQConsumeOkTagMissmatchException(string waitngTag, string tag)
            : base($"ConsumeOk tag missmatch: waiting:{waitngTag} received:{tag}")
        {
        }
    }
}
