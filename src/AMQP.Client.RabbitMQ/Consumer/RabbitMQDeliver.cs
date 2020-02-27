using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public readonly struct RabbitMQDeliver
    {
        public readonly DeliverInfo Info;
        private readonly RabbitMQProtocol _protocol;
        private readonly ushort _channelId;
        private readonly SemaphoreSlim _writerSemaphore;
        public RabbitMQDeliver(DeliverInfo info, ushort channelId, RabbitMQProtocol protocol, SemaphoreSlim semaphore)
        {
            Info = info;
            _protocol = protocol;
            _channelId = channelId;
            _writerSemaphore = semaphore;
        }
        public async ValueTask Ack(bool multiple = false)
        {
            await _writerSemaphore.WaitAsync().ConfigureAwait(false);
            await _protocol.Writer.WriteAsync(new BasicAckWriter(_channelId), new AckInfo(Info.DeliverTag, multiple)).ConfigureAwait(false);
            _writerSemaphore.Release();
        }
        public async ValueTask Reject(bool requeue)
        {
            await _writerSemaphore.WaitAsync().ConfigureAwait(false);
            await _protocol.Writer.WriteAsync(new BasicRejectWriter(_channelId), new RejectInfo(Info.DeliverTag, requeue)).ConfigureAwait(false); ;
            _writerSemaphore.Release();
        }
    }
}
