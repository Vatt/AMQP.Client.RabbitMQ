namespace AMQP.Client.RabbitMQ.Protocol.Exceptions
{
    public class RabbitMQFrameException : RabbitMQException
    {
        public RabbitMQFrameException(byte frameType) : base($"Cannot read frame, frame type = {frameType}")
        {

        }
    }
}
