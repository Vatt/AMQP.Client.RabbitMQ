using System;
using AMQP.Client.RabbitMQ.Exceptions;
using AMQP.Client.RabbitMQ.Protocol.Exceptions;

namespace AMQP.Client.RabbitMQ.Internal
{
    internal class RabbitMQExceptionHelper
    {
        internal static void ThrowChannelNotFound(ushort channelId)
        {
            throw new RabbitMQChannelNotFoundException(channelId);
        }

        internal static void ThrowConsumeOkTagMissmatch(string waitngTag, string tag)
        {
            throw new RabbitMQConsumeOkTagMissmatchException(waitngTag, tag);
        }

        internal static void ThrowConnectionClosed(Guid id)
        {
            throw new ConnectionClosedException(id);
        }
    }
}