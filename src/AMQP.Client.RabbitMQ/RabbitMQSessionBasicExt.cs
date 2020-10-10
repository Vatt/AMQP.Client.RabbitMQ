﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;

namespace AMQP.Client.RabbitMQ
{
    internal static class RabbitMQSessionBasicExt
    {
        internal static async Task ConsumerStartAsync(this RabbitMQSession session, RabbitMQConsumer consumer)
        {
            var data = session.GetChannelData(consumer.Channel.ChannelId);
            if (!consumer.Conf.NoWait)
            {
                data.ConsumeTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            await session.Writer.SendBasicConsumeAsync(consumer.Channel.ChannelId, consumer.Conf).ConfigureAwait(false);
            if (!consumer.Conf.NoWait)
            {
                var tag = await data.ConsumeTcs.Task.ConfigureAwait(false);
                if (!tag.Equals(consumer.Conf.ConsumerTag))
                {
                    RabbitMQExceptionHelper.ThrowIfConsumeOkTagMissmatch(consumer.Conf.ConsumerTag, tag);
                }
                consumer.IsClosed = false;
            }
            data.Consumers.Add(consumer.Conf.ConsumerTag, consumer);
        }

        internal static async Task QoS(this RabbitMQSession session, RabbitMQChannel channel, QoSInfo qos)
        {
            var data = session.GetChannelData(channel.ChannelId);
            data.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await session.Writer.SendBasicQoSAsync(channel.ChannelId, ref qos).ConfigureAwait(false);
            await data.CommonTcs.Task.ConfigureAwait(false);
        }
        public static ValueTask PublishBodyAsync(this RabbitMQSession session, ushort channelId, IEnumerable<ReadOnlyMemory<byte>> batch, CancellationToken token = default)
        {
            //return session.Writer.WriteManyAsync(new BodyFrameWriter(channelId), batch, token);
            return default;
        }
    }
}