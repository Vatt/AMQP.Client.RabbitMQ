using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQProtocolReader : IAsyncDisposable
    {
        private readonly byte[] _buffer;
        private readonly FrameReader _frameReader = new FrameReader();
        private readonly ProtocolReader Reader;

        private long _bufferPosition;
        //private bool _needAdvance;

        public RabbitMQProtocolReader(ConnectionContext ctx, ref TuneConf tune)
        {
            Reader = ctx.CreateReader();
            _buffer = new byte[tune.FrameMax];
        }

        public ValueTask DisposeAsync()
        {
            return Reader.DisposeAsync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Advance()
        {
            //Reader.Advance();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<T> ReadAsync<T>(IMessageReader<T> reader, CancellationToken token = default)
        {
            var result = await Reader.ReadAsync(reader, token).ConfigureAwait(false);
            Reader.Advance();
            if (result.IsCanceled || result.IsCanceled)
            {
                //TODO: do something
            }

            return result.Message;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<ReadOnlySequence<byte>> ReadAsync(CancellationToken token = default)
        {
            while (!_frameReader.IsComplete)
            {
                var result = await Reader.ReadAsync(_frameReader, token).ConfigureAwait(false);
                CopyResultToBuffer(result.Message);
                Reader.Advance();
            }

            var message = new ReadOnlySequence<byte>(_buffer, 0, _frameReader.FrameSize);
            _frameReader.Reset();
            _bufferPosition = 0;

            return message;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CopyResultToBuffer(ReadOnlySequence<byte> source)
        {
            var span = new Span<byte>(_buffer, (int) _bufferPosition, (int) source.Length);
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
    }
}