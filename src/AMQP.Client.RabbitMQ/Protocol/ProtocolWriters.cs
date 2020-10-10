using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;

namespace AMQP.Client.RabbitMQ.Protocol
{
    internal static class ProtocolWriters
    {
        public static readonly NoPayloadMethodWriter NoPayloadMethodWriter = new NoPayloadMethodWriter();
        public static readonly CloseWriter CloseWriter = new CloseWriter();
        public static readonly ContentHeaderWriter ContentHeaderWriter = new ContentHeaderWriter();
        public static readonly BasicPublishWriter BasicPublishWriter = new BasicPublishWriter();
        public static readonly BodyFrameWriter BodyFrameWriter = new BodyFrameWriter();
        public static readonly QueueBindWriter QueueBindWriter = new QueueBindWriter();
        public static readonly QueueDeclareWriter QueueDeclareWriter = new QueueDeclareWriter();
        public static readonly QueueDeleteWriter QueueDeleteWriter = new QueueDeleteWriter();
        public static readonly QueuePurgeWriter QueuePurgeWriter = new QueuePurgeWriter();
        public static readonly QueueUnbindWriter QueueUnbindWriter = new QueueUnbindWriter();
        public static readonly ExchangeDeclareWriter ExchangeDeclareWriter = new ExchangeDeclareWriter();
        public static readonly ExchangeDeleteWriter ExchangeDeleteWriter = new ExchangeDeleteWriter();
        public static readonly BasicAckWriter BasicAckWriter = new BasicAckWriter();
        public static readonly BasicQoSWriter BasicQoSWriter = new BasicQoSWriter();
        public static readonly BasicRejectWriter BasicRejectWriter = new BasicRejectWriter();
        public static readonly BasicConsumeCancelWriter BasicConsumeCancelWriter = new BasicConsumeCancelWriter();
        public static readonly BasicConsumeWriter BasicConsumeWriter = new BasicConsumeWriter();
        public static readonly ConnectionOpenWriter ConnectionOpenWriter = new ConnectionOpenWriter();
        public static readonly ConnectionStartOkWriter ConnectionStartOkWriter = new ConnectionStartOkWriter();
        public static readonly ConnectionTuneOkWriter ConnectionTuneOkWriter = new ConnectionTuneOkWriter();

    }
}