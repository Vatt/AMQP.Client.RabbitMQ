using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQProtocolReader : IAsyncDisposable
    {
        private readonly ProtocolReader _protocol;
        private readonly FrameReader _frameReader = new FrameReader();
        private readonly byte[] _buffer;
        private long _bufferPosition;
        public RabbitMQProtocolReader(ConnectionContext ctx)
        {
            _protocol = ctx.CreateReader();
            _buffer = new byte[128 * 1024];
        }

        public ValueTask DisposeAsync()
        {
            return _protocol.DisposeAsync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Advance()
        {
            _protocol.Advance();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<T> ReadAsync<T>(IMessageReader<T> reader, CancellationToken token = default)
        {
            var result = await _protocol.ReadAsync(reader, token).ConfigureAwait(false);
            _protocol.Advance();
            if (result.IsCanceled || result.IsCompleted)
            {
                //TODO: do something
            }

            return result.Message;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T Read<T>(IMessageReaderAdapter<T> reader, in ReadOnlySequence<byte> input)
        {
            var tryRead = reader.TryParseMessage(input, out var info);
            if (!tryRead) ReaderThrowHelper.ThrowIfCantProtocolRead();

            return info;
        }
        internal async ValueTask<T> ReadWithoutAdvanceAsync<T>(IMessageReader<T> reader, CancellationToken token = default)
        {
            var result = await _protocol.ReadAsync(reader, token).ConfigureAwait(false);
            if (result.IsCanceled || result.IsCanceled)
            {
                //TODO: do something
            }
            return result.Message;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //internal async ValueTask<ReadOnlySequence<byte>> ReadAsync(CancellationToken token = default)
        //{
        //    while (!_frameReader.IsComplete)
        //    {
        //        var result = await _protocol.ReadAsync(_frameReader, token).ConfigureAwait(false);
        //        CopyResultToBuffer(result.Message);
        //        _protocol.Advance();
        //        /*
        //        if (result.IsCanceled || result.IsCanceled)
        //        {
        //            //TODO: do something
        //        }
        //        if (frameReader.IsComplete && frameReader.IsSingle)
        //        {
        //            _bufferPosition = 0;
        //            _needAdvance = true;
        //            return result.Message;
        //        }
        //        else
        //        {
        //            CopyResultToBuffer(result.Message);
        //            Reader.Advance();
        //        }

        //        CopyResultToBuffer(result.Message);
        //        Reader.Advance();
        //        */
        //    }

        //    var message = new ReadOnlySequence<byte>(_buffer, 0, _frameReader.FrameSize);
        //    _frameReader.Reset();
        //    _bufferPosition = 0;

        //    return message;
        //}
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private void CopyResultToBuffer(ReadOnlySequence<byte> source)
        //{
        //    var span = new Span<byte>(_buffer, (int)_bufferPosition, (int)source.Length);
        //    source.CopyTo(span);
        //    _bufferPosition += source.Length;
        //}
    }
}