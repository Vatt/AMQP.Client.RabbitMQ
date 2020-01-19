using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Decoder
{
    internal class DecoderThrowHelper
    {
        public static void  ThrowValueDecoderStringDecodeFailed()
        {
            throw new Exception("ValueDecoder: string decode failed ");
        }
        public static void ThrowValueDecoderUnrecognisedType()
        {
            throw new ArgumentException("Unrecognised type");
        }
        public static void ThrowFrameDecoderStartMethodDecodeFailed()
        {
            throw new Exception("FrameDecoder: start method decode failed");
        }
        public static void ThrowFrameDecoderEndMarkerMissmatch()
        {
            throw new Exception("FrameDecoder: end-marker missmatch");
        }
        public static void ThrowFrameDecoderAMQPVersionMissmatch()
        {
            throw new Exception("FrameDecoder: AMQP version missmatch");
        }
    }
}
