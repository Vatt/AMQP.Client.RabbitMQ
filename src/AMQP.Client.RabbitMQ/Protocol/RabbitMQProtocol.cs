using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQProtocol
    {
        private readonly ProtocolReader Reader;
        private readonly ProtocolWriter Writer;
        public RabbitMQProtocol(ConnectionContext ctx)
        {
            Reader = ctx.CreateReader();
            Writer = ctx.CreateWriter();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<T> ReadAsync<T>(IMessageReader<T> reader, CancellationToken token = default)
        {
            var result = await Reader.ReadAsync(reader, token);
            Reader.Advance();
            if (result.IsCanceled || result.IsCanceled)
            {
                //TODO: do something
            }
            return result.Message;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<T> ReadWithoutAdvanceAsync<T>(IMessageReader<T> reader, CancellationToken token = default)
        {
            var result = await Reader.ReadAsync(reader, token);
            if (result.IsCanceled || result.IsCanceled)
            {
                //TODO: do something
            }
            return result.Message;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ReaderAdvance()
        {
            Reader.Advance();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ValueTask WriteAsync<T>(IMessageWriter<T> writer, T message, CancellationToken token = default)
        {
            return Writer.WriteAsync(writer, message, token);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ValueTask WriteManyAsync<T>(IMessageWriter<T> writer, IEnumerable<T> messages, CancellationToken token = default)
        {
            return Writer.WriteManyAsync(writer, messages, token);
        }

    }
}
