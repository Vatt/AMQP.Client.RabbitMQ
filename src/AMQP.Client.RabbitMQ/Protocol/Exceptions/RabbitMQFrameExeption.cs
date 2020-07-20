namespace AMQP.Client.RabbitMQ.Protocol.Exceptions
{
    public class RabbitMQFrameExeption : RabbitMQException
    {
        public RabbitMQFrameExeption(byte frameType) : base($"Cannot read frame, frame type = {frameType}")
        {

        }
    }
}
