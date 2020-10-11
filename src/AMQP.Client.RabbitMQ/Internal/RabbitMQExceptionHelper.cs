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
        internal static void ThrowConnectionClosed(ushort channelId)
        {
            throw new ChannelClosedException(channelId);
        }
        internal static void ThrowConnectionInProgress(Guid id)
        {
            throw new InvalidOperationException($"Connection {id} in progress");
        }
    }
}