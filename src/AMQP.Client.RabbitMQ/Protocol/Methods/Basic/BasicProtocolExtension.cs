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
        public static ValueTask SendBasicConsume(this RabbitMQProtocol protocol, ushort channelId, ConsumerInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new BasicConsumeWriter(channelId), info, token);
        }
        public static ValueTask<string> ReadBasicConsumeOk(this RabbitMQProtocol protocol)
        {
            return protocol.ReadShortStrPayload();
        }
        public static ValueTask<DeliverInfo> ReadBasicDeliver(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_basicDeliverReader, token);
        }
        public static ValueTask SendBasicQoS(this RabbitMQProtocol protocol, ushort channelId, ref QoSInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new BasicQoSWriter(channelId), info, token);
        }
        public static ValueTask<bool> ReadBasicQoSOk(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadNoPayload(token);
        }
        public static ValueTask SendReject(this RabbitMQProtocol protocol, ushort channelId, ref RejectInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new BasicRejectWriter(channelId), info, token);
        }
        public static ValueTask SendAck(this RabbitMQProtocol protocol, ushort channelId, ref AckInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new BasicAckWriter(channelId), info, token);
        }
        public static ValueTask<ContentHeader> ReadContentHeaderWithFrameHeader(this RabbitMQProtocol protocol, ushort channelId, CancellationToken token = default)
        {
            var reader = new ContentHeaderFullReader(channelId);
            return protocol.ReadAsync(reader, token);
        }
        public static ValueTask PublishAll(this RabbitMQProtocol protocol, ushort channelId, PublishAllInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new PublishFullWriter(channelId), info, token);
        }
        public static ValueTask PublishPartial(this RabbitMQProtocol protocol, ushort channelId, PublishPartialInfo info, CancellationToken token = default)
        {
            var writer = new PublishInfoAndContentWriter(channelId);
            return protocol.WriteAsync(writer, info, token);
        }
        public static ValueTask PublishBody(this RabbitMQProtocol protocol, ushort channelId, IEnumerable<ReadOnlyMemory<byte>> batch, CancellationToken token = default)
        {
            return protocol.WriteManyAsync(new BodyFrameWriter(channelId), batch, token);
        }
    }
}
