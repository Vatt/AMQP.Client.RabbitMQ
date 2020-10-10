using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public readonly struct ConsumeConf
    {
        public readonly string QueueName;
        public readonly string ConsumerTag;
        public readonly Dictionary<string, object> Arguments;
        public readonly ushort ChannelId;
        public readonly bool NoLocal;
        public readonly bool NoAck;
        public readonly bool Exclusive;
        public readonly bool NoWait;
        internal ConsumeConf(ushort channelId, string queue, string tag, bool noLocal = false, bool noAck = false, bool exclusive = false, bool nowait = false, Dictionary<string, object> arguments = null)
        {
            ChannelId = channelId;
            QueueName = queue;
            ConsumerTag = tag;
            NoLocal = noLocal;
            NoAck = noAck;
            Exclusive = exclusive;
            NoWait = nowait;
            Arguments = arguments;
        }
        public static ConsumeConf Create(ushort channelId, string queueName, string consumerTag, bool noLocal = false, bool noAck = false, bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            return new ConsumeConf(channelId, queueName, consumerTag, noLocal, noAck, exclusive, false, arguments);
        }
        public static ConsumeConf Create(ushort channelId, string queueName, string consumerTag, bool noAck)
        {
            return new ConsumeConf(channelId, queueName, consumerTag, false, noAck, false, false, null);
        }
        public static ConsumeConf CreateNoWait(ushort channelId, string queueName, string consumerTag, bool noLocal = false, bool noAck = false, bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            return new ConsumeConf(channelId, queueName, consumerTag, noLocal, noAck, exclusive, true, arguments);
        }
        public static ConsumeConf CreateNoWait(ushort channelId, string queueName, string consumerTag, bool noAck = false)
        {
            return new ConsumeConf(channelId, queueName, consumerTag, false, noAck, false, true, null);
        }
    }
}
