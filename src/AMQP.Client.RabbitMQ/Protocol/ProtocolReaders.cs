using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Channel;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;

namespace AMQP.Client.RabbitMQ.Protocol
{
    internal static class ProtocolReaders
    {
        public static readonly CloseReader CloseReader = new CloseReader();
        public static readonly BasicConsumeCancelReader BasicConsumeCancelReader = new BasicConsumeCancelReader();
        public static readonly BasicDeliverReader BasicDeliverReader = new BasicDeliverReader();
        public static readonly ChannelOpenOkReader ChannelOpenOkReader = new ChannelOpenOkReader();
        public static readonly ConnectionOpenOkReader ConnectionOpenOkReader = new ConnectionOpenOkReader();
        public static readonly ConnectionStartReader ConnectionStartReader = new ConnectionStartReader();
        public static readonly ConnectionTuneReader ConnectionTuneReader = new ConnectionTuneReader();
        public static readonly QueueDeclareOkReader QueueDeclareOkReader = new QueueDeclareOkReader();
        public static readonly QueuePurgeOkDeleteOkReader QueuePurgeOkDeleteOkReader = new QueuePurgeOkDeleteOkReader();
        public static readonly FrameHeaderReader FrameHeaderReader = new FrameHeaderReader();
        public static readonly MethodHeaderReader MethodHeaderReader = new MethodHeaderReader();
        public static readonly NoPayloadReader NoPayloadReader = new NoPayloadReader();
        public static readonly ShortStrPayloadReader ShortStrPayloadReader = new ShortStrPayloadReader();
    
    }
}