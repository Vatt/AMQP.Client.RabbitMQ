using System;

namespace AMQP.Client.RabbitMQ.Internal
{
    internal class RabbitMQExceptionHelper
    {
        internal static void ThrowIfChannelNotFound()
        {
            throw new Exception("Channel not found");
        }
        internal static void ThrowIfConsumeOkTagMissmatch(string waitngTag, string tag)
        {
            throw new Exception($"ConsumeOk tag missmatch: waiting:{waitngTag} received:{tag} ");
        }
    }
}