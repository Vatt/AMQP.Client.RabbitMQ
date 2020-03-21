using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    public interface IChunkedBodyFrameReader : IMessageReader<ReadOnlySequence<byte>>
    {
        bool IsComplete { get; }
        void Reset(long contentBodySize);
    }
    internal class BodyFrameChunkedReader : IChunkedBodyFrameReader
    {
        public long _localConsumed = 0;
        public long _contentConsumed = 0;
        private int _payloadSize;
        private long _contentBodySize;
        private readonly ushort _channelId;
        private bool needHead;
        public bool IsComplete => _contentConsumed == _contentBodySize;
        public BodyFrameChunkedReader(ushort channelId)
        {
            _channelId = channelId;
        }
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out ReadOnlySequence<byte> message)
        {
            message = default;
            if (input.Length == 0) { return false; }
            ValueReader reader = new ValueReader(input);

            if (needHead)
            {
                if (!ReadHeader(out var type, out ushort channel, out _payloadSize, ref reader))
                {
                    return false;
                }
                if (type != Constants.FrameBody || channel != _channelId)
                {
                    throw new Exception($"{nameof(BodyFrameChunkedReader)}: frame type or channel id missmatch");
                }

            }

            var readable = Math.Min((_payloadSize - _localConsumed), reader.Remaining);
            message = input.Slice(reader.Position, readable);
            _localConsumed += readable;
            _contentConsumed += readable;
            reader.Advance(readable);

            if (_localConsumed == _payloadSize)
            {
                if (!reader.ReadOctet(out byte marker))
                {
                    _localConsumed -= readable;
                    _contentConsumed -= readable;
                    return false;
                }
                if (marker != Constants.FrameEnd)
                {
                    ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
                }
                consumed = reader.Position;
                examined = consumed;
                needHead = true;
                _localConsumed = 0;
                return true;
            }
            if (needHead) { needHead = false; }
            consumed = reader.Position;
            examined = consumed;
            return true;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ReadHeader(out byte type, out ushort channel, out int payloadSize, ref ValueReader reader)
        {
            channel = default;
            payloadSize = default;
            if (!reader.ReadOctet(out type))
            {
                return false;
            }
            if (!reader.ReadShortInt(out channel))
            {
                return false;
            }
            if (!reader.ReadLong(out payloadSize))
            {
                return false;
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset(long contentBodySize)
        {
            _localConsumed = 0;
            _contentConsumed = 0;
            needHead = true;
            _contentBodySize = contentBodySize;
        }
    }
}
