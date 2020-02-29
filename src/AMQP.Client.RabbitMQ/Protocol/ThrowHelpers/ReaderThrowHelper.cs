using System;

namespace AMQP.Client.RabbitMQ.Protocol.ThrowHelpers
{
    internal class ReaderThrowHelper
    {
        public static void ThrowIfUnrecognisedType()
        {
            throw new ArgumentException("Unrecognised type");
        }
        public static void ThrowIfEndMarkerMissmatch()
        {
            throw new Exception("End-marker missmatch");
        }
        public static void ThrowIfAMQPVersionMissmatch()
        {
            throw new Exception("AMQP version missmatch");
        }
    }
}
