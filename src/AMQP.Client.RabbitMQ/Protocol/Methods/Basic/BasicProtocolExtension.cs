using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public static class BasicProtocolExtension
    {
        private static readonly BasicDeliverReader _basicDeliverReader = new BasicDeliverReader();
        public static ValueTask SendBasicConsumeAsync(this RabbitMQProtocol protocol, ushort channelId, ConsumerInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new BasicConsumeWriter(channelId), info, token);
        }
        public static ValueTask<string> ReadBasicConsumeOkAsync(this RabbitMQProtocol protocol)
        {
            return protocol.ReadShortStrPayload();
        }
        public static ValueTask<DeliverInfo> ReadBasicDeliverAsync(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_basicDeliverReader, token);
        }
        public static ValueTask SendBasicQoSAsync(this RabbitMQProtocol protocol, ushort channelId, ref QoSInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new BasicQoSWriter(channelId), info, token);
        }
        public static ValueTask<bool> ReadBasicQoSOkAsync(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadNoPayload(token);
        }
        public static ValueTask SendRejectAsync(this RabbitMQProtocol protocol, ushort channelId, ref RejectInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new BasicRejectWriter(channelId), info, token);
        }
        public static ValueTask SendAckAsync(this RabbitMQProtocol protocol, ushort channelId, ref AckInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new BasicAckWriter(channelId), info, token);
        }
        public static ValueTask<ContentHeader> ReadContentHeaderWithFrameHeaderAsync(this RabbitMQProtocol protocol, ushort channelId, CancellationToken token = default)
        {
            var reader = new ContentHeaderFullReader(channelId);
            return protocol.ReadAsync(reader, token);
        }
        public static ValueTask PublishAllAsync(this RabbitMQProtocol protocol, ushort channelId, PublishAllInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new PublishFullWriter(channelId), info, token);
        }
        public static ValueTask PublishPartialAsync(this RabbitMQProtocol protocol, ushort channelId, PublishPartialInfo info, CancellationToken token = default)
        {
            var writer = new PublishInfoAndContentWriter(channelId);
            return protocol.WriteAsync(writer, info, token);
        }
        public static ValueTask PublishBodyAsync(this RabbitMQProtocol protocol, ushort channelId, IEnumerable<ReadOnlyMemory<byte>> batch, CancellationToken token = default)
        {
            return protocol.WriteManyAsync(new BodyFrameWriter(channelId), batch, token);
        }
    }
}
