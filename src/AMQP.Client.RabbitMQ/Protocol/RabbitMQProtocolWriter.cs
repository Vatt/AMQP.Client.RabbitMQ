using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQProtocolWriter : IAsyncDisposable
    {
        private readonly ProtocolWriter Writer;
        public RabbitMQProtocolWriter(ConnectionContext ctx)
        {
            Writer = ctx.CreateWriter();
        }

        public ValueTask DisposeAsync()
        {
            return Writer.DisposeAsync();
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