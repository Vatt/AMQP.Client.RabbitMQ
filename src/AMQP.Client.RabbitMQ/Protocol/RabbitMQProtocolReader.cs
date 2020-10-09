using System;
using System.Buffers;
using System.Net.Connections;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQProtocolReader : IAsyncDisposable
    {
        private readonly ProtocolReader _protocol;
        public RabbitMQProtocolReader(Connection ctx)
        {
            _protocol = ctx.CreateReader();
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
    }
}