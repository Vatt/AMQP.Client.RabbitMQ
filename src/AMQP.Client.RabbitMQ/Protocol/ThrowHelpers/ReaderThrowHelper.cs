using AMQP.Client.RabbitMQ.Protocol.Exceptions;

namespace AMQP.Client.RabbitMQ.Protocol.ThrowHelpers
{
    internal class ReaderThrowHelper
    {
        public static void ThrowIfUnrecognisedType()
        {
            throw new RabbitMQException("Unrecognised type");
        }
        public static void ThrowIfEndMarkerMissmatch()
        {
            throw new RabbitMQException("End-marker missmatch");
        }
        public static void ThrowIfAMQPVersionMissmatch()
        {
            throw new RabbitMQException("AMQP version missmatch");
        }
        public static void ThrowIfFrameTypeMissmatch()
        {
            throw new RabbitMQException("Frame type missmatch");
        }
        public static void ThrowIfCantProtocolRead()
        {
            throw new RabbitMQException("Cant read message");
        }
    }
}
