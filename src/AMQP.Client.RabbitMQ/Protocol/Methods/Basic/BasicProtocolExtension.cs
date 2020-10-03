using AMQP.Client.RabbitMQ.Protocol.Common;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public static class BasicProtocolExtension
    {
        private static readonly BasicDeliverReader _basicDeliverReader = new BasicDeliverReader();
        private static readonly BasicConsumeCancelReader _consumeCancelReader = new BasicConsumeCancelReader();
        private static readonly PublishFullWriter _fullWriter = new PublishFullWriter();
        public static ValueTask SendBasicConsumeAsync(this RabbitMQProtocolWriter protocol, ushort channelId, ConsumeConf info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new BasicConsumeWriter(channelId), info, token);
        }
        public static string ReadBasicConsumeOk(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.ReadShortStrPayload(input);
        }
        public static ValueTask<string> ReadBasicConsumeOkAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadShortStrPayloadAsync(token);
        }
        public static string ReadBasicConsumeCancelOk(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.ReadShortStrPayload(input);
        }
        public static ValueTask<string> ReadBasicConsumeCancelOkAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadShortStrPayloadAsync(token);
        }
        public static ValueTask<ConsumeCancelInfo> ReadBasicConsumeCancelAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_consumeCancelReader, token);
        }
        public static ValueTask<RabbitMQDeliver> ReadBasicDeliverAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_basicDeliverReader, token);
        }

        public static RabbitMQDeliver ReadBasicDeliver(this RabbitMQProtocolReader protocol, ReadOnlySequence<byte> input)
        {
            return protocol.Read(_basicDeliverReader, input);
        }
        public static ValueTask SendBasicQoSAsync(this RabbitMQProtocolWriter protocol, ushort channelId, ref QoSInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new BasicQoSWriter(channelId), info, token);
        }
        public static bool ReadBasicQoSOkAsync(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.ReadNoPayload(input);
        }
        public static ValueTask<bool> ReadBasicQoSOkAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadNoPayloadAsync(token);
        }
        public static ValueTask SendRejectAsync(this RabbitMQProtocolWriter protocol, ushort channelId, ref RejectInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new BasicRejectWriter(channelId), info, token);
        }
        public static ValueTask SendAckAsync(this RabbitMQProtocolWriter protocol, ushort channelId, ref AckInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new BasicAckWriter(channelId), info, token);
        }
        /*
        public static ValueTask<ContentHeader> ReadContentHeaderWithFrameHeaderAsync(this RabbitMQProtocolReader protocol, ushort channelId, CancellationToken token = default)
        {
            var reader = new ContentHeaderFullReader(channelId);
            return protocol.ReadAsync(reader, token);
        }
        */
        public static ValueTask PublishAllAsync(this RabbitMQProtocolWriter protocol, ushort channelId, ref PublishAllInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(_fullWriter, info, token);
        }
        public static ValueTask PublishPartialAsync(this RabbitMQProtocolWriter protocol, ushort channelId, PublishPartialInfo info, CancellationToken token = default)
        {
            var writer = new PublishInfoAndContentWriter(channelId);
            return protocol.WriteAsync(writer, info, token);
        }

        public static ValueTask PublishBodyAsync(this RabbitMQProtocolWriter protocol, ushort channelId, IEnumerable<ReadOnlyMemory<byte>> batch, CancellationToken token = default)
        {
            return protocol.WriteManyAsync(new BodyFrameWriter(channelId), batch, token);
        }
        public static IChunkedBodyFrameReader CreateResetableChunkedBodyReader(this RabbitMQProtocolReader protocol, ushort channelId)
        {
            return new BodyFrameChunkedReader(channelId);
        }
    }
}
