using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.ThrowHelpers
{
    internal class ReaderThrowHelper
    {
        public static void ThrowIfValueDecoderUnrecognisedType()
        {
            throw new ArgumentException("Unrecognised type");
        }
        public static void ThrowIfFrameDecoderEndMarkerMissmatch()
        {
            throw new Exception("FrameDecoder: end-marker missmatch");
        }
        public static void ThrowIfFrameDecoderAMQPVersionMissmatch()
        {
            throw new Exception("FrameDecoder: AMQP version missmatch");
        }
    }
}
