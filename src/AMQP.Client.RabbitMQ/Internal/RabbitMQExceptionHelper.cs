using AMQP.Client.RabbitMQ.Protocol.Exceptions;

namespace AMQP.Client.RabbitMQ.Internal
{
    internal class RabbitMQExceptionHelper
    {
        internal static void ThrowIfChannelNotFound(ushort channelId)
        {
            throw new RabbitMQChannelNotFoundException(channelId);
        }

        internal static void ThrowIfConsumeOkTagMissmatch(string waitngTag, string tag)
        {
            throw new RabbitMQConsumeOkTagMissmatchException(waitngTag, tag);
        }
    }
}