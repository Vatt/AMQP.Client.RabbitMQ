using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Internal;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQProtocolReader : IAsyncDisposable, IMessageReader<ReadOnlySequence<byte>>
    {
        private readonly byte[] _buffer;
        private readonly FrameReader _frameReader = new FrameReader();
        private readonly ProtocolReader _protocol;
        private readonly Memory<byte> _memory1;
        private readonly Memory<byte> _memory2;
        private long _bufferPosition;
        private int _frameMax;
        private bool _memory1InPorcess;
        private bool _memory2InPorcess;
        private int _consumed;
        public int FrameSize { get; private set; }
        public bool IsComplete { get; private set; }
        //private bool _needAdvance;

        public RabbitMQProtocolReader(ConnectionContext ctx, ref TuneConf tune)
        {
            _protocol = ctx.CreateReader();
            _frameMax = tune.FrameMax;
            _buffer = new byte[_frameMax * 2];
            _memory1 = new Memory<byte>(_buffer, 0, tune.FrameMax);
            _memory2 = new Memory<byte>(_buffer, tune.FrameMax, tune.FrameMax);
            _memory1InPorcess = false;
            _memory2InPorcess = false;
        }

        public ValueTask DisposeAsync()
        {
            return _protocol.DisposeAsync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Advance()
        {
            //Reader.Advance();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<T> ReadAsync<T>(IMessageReader<T> reader, CancellationToken token = default)
        {
            var result = await _protocol.ReadAsync(reader, token).ConfigureAwait(false);
            _protocol.Advance();
            if (result.IsCanceled || result.IsCanceled)
            {
                //TODO: do something
            }

            return result.Message;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<ReadOnlySequence<byte>> ReadAsync(CancellationToken token = default)
        {
            //while (!_frameReader.IsComplete)
            while (!IsComplete)
            {
                var result = await _protocol.ReadAsync(this, token).ConfigureAwait(false);
                CopyResultToBuffer(result.Message);
                _protocol.Advance();
            }

            ReadOnlySequence<byte> message = default;
            if (_memory1InPorcess)
            {
                message = new ReadOnlySequence<byte>(_memory2);
                _memory2InPorcess = true;
            }else if (_memory2InPorcess)
            {
                message = new ReadOnlySequence<byte>(_memory1);
                _memory1InPorcess = true;
            }


            //_frameReader.Reset();
            Reset();
            _bufferPosition = 0;

            return message;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CopyResultToBuffer(ReadOnlySequence<byte> source)
        {
            Span<byte> span = default;
            if (_memory1InPorcess)
            {
                span = new Span<byte>(_buffer, _frameMax + (int)_bufferPosition, (int)source.Length);
            }else if (_memory2InPorcess)
            {
                span = new Span<byte>(_buffer, (int)_bufferPosition, (int)source.Length);
            }
            source.CopyTo(span);
            _bufferPosition += source.Length;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T Read<T>(IMessageReaderAdapter<T> reader, in ReadOnlySequence<byte> input)
        {
            var tryRead = reader.TryParseMessage(input, out var info);
            if (!tryRead) ReaderThrowHelper.ThrowIfCantProtocolRead();

            return info;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined,
            out ReadOnlySequence<byte> message)
        {
            message = default;
            if (_memory1InPorcess && _memory2InPorcess)
            {
                return false;
            }

            if (input.IsEmpty)
            {
                return false;
            }
            var reader = new ValueReader(input);
            if (_consumed == 0)
            {
                if (!ReadHeader(out var type, out var channel, out var payloadSize, ref reader)) return false;
                FrameSize = 7 + payloadSize + 1; // header + payload + endMarker
                if (input.Length >= FrameSize)
                {
                    message = input.Slice(0, FrameSize);
                    reader.Advance(FrameSize - reader.Consumed);
                    consumed = reader.Position;
                    examined = consumed;
                    IsComplete = true;
                    return true;
                }
            }


            var readable = Math.Min(FrameSize - _consumed, input.Length);
            message = input.Slice(0, readable);
            _consumed += (int)readable;
            reader.Advance(readable - reader.Consumed);
            if (_consumed == FrameSize)
            {
                //if (!reader.ReadOctet(out byte marker) || marker != Constants.FrameEnd)
                //{
                //    ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
                //}
                consumed = reader.Position;
                examined = consumed;
                IsComplete = true;
                return true;
            }

            consumed = reader.Position;
            examined = consumed;
            return true;
        }
        public void Reset()
        {
            _consumed = 0;
            FrameSize = 0;
            IsComplete = false;
        }

        private bool ReadHeader(out byte type, out ushort channel, out int payloadSize, ref ValueReader reader)
        {
            channel = default;
            payloadSize = default;
            if (!reader.ReadOctet(out type)) return false;
            if (!reader.ReadShortInt(out channel)) return false;
            if (!reader.ReadLong(out payloadSize)) return false;
            return true;
        }
    }
}