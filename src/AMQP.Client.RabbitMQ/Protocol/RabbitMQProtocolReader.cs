using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQProtocolReader
    {
        private ProtocolReader Reader;

        public RabbitMQProtocolReader(ConnectionContext ctx)
        {
            Reader = ctx.CreateReader();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<T> ReadWithoutAdvanceAsync<T>(IMessageReader<T> reader, CancellationToken token = default)
        {
            var result = await Reader.ReadAsync(reader, token).ConfigureAwait(false);
            if (result.IsCanceled || result.IsCanceled)
            {
                //TODO: do something
            }
            return result.Message;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Advance()
        {
            Reader.Advance();
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
        internal T Read<T>(IMessageReaderAdapter<T> reader, in ReadOnlySequence<byte> input)
        {
            var try_read = reader.TryParseMessage(input, out var info);
            if (!try_read)
            {
                ReaderThrowHelper.ThrowIfCantProtocolRead();
            }
            return info;
        }
    }
}
