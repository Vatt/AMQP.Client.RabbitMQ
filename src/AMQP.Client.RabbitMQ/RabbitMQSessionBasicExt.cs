using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    internal static class RabbitMQSessionBasicExt
    {
        private static readonly PublishFullWriter _fullWriter = new PublishFullWriter();
        internal static async Task ConsumerStartAsync(this RabbitMQSession session, RabbitMQConsumer consumer)
        {
            var data = session.GetChannelData(consumer.Channel.ChannelId);
            data.ConsumeTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            await session.Writer.SendBasicConsumeAsync(consumer.Channel.ChannelId, consumer.Conf).ConfigureAwait(false);
            var tag = await data.ConsumeTcs.Task.ConfigureAwait(false);
            if (!tag.Equals(consumer.Conf.ConsumerTag))
            {
                RabbitMQExceptionHelper.ThrowIfConsumeOkTagMissmatch(consumer.Conf.ConsumerTag, tag);
            }
            consumer.IsClosed = false;
            data.Consumers.Add(tag, consumer);
        }

        internal static async Task QoS(this RabbitMQSession session, RabbitMQChannel channel, QoSInfo qos)
        {
            var data = session.GetChannelData(channel.ChannelId);
            data.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await session.Writer.SendBasicQoSAsync(channel.ChannelId, ref qos).ConfigureAwait(false);
            await data.CommonTcs.Task.ConfigureAwait(false);
        }
        internal async static ValueTask PublishAllAsync(this RabbitMQSession session, PublishAllInfo info, CancellationToken token = default)
        {
            try
            {
                if (!session.ConnectionGlobalLock.Task.IsCompleted && !session.ConnectionGlobalLock.Task.IsCanceled)
                {
                    session.Logger.LogCritical($"{nameof(RabbitMQSession)}: PublishAllAsync waiting");
                }
                await session.ConnectionGlobalLock.Task.ConfigureAwait(false);
                //session.ConnectionGlobalLock.Task.Wait();
            }
            catch(Exception e)
            {
                Debugger.Break();
            }
            
            await session.Writer.WriteAsync(_fullWriter, info, token);
        }
        internal async static ValueTask PublishPartialAsync(this RabbitMQSession session, ushort channelId, PublishPartialInfo info, CancellationToken token = default)
        {
            await session.ConnectionGlobalLock.Task.ConfigureAwait(false);
            var writer = new PublishInfoAndContentWriter(channelId);
            await session.Writer.WriteAsync(writer, info, token);
        }
        public async static ValueTask PublishBodyAsync(this RabbitMQSession session, ushort channelId, IEnumerable<ReadOnlyMemory<byte>> batch, CancellationToken token = default)
        {
            await session.ConnectionGlobalLock.Task.ConfigureAwait(false);
            await session.Writer.WriteManyAsync(new BodyFrameWriter(channelId), batch, token);
        }
    }
}