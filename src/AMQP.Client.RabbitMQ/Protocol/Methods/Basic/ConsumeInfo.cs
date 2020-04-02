using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public readonly struct ConsumerConf
    {
        public readonly string QueueName;
        public readonly string ConsumerTag;
        public readonly Dictionary<string, object> Arguments;
        public readonly bool NoLocal;
        public readonly bool NoAck;
        public readonly bool Exclusive;
        public readonly bool NoWait;
        internal ConsumerConf(string queue, string tag, bool noLocal = false, bool noAck = false, bool exclusive = false, bool nowait = false, Dictionary<string, object> arguments = null)
        {
            QueueName = queue;
            ConsumerTag = tag;
            NoLocal = noLocal;
            NoAck = noAck;
            Exclusive = exclusive;
            NoWait = nowait;
            Arguments = arguments;
        }
        public static ConsumerConf Create(string queueName, string consumerTag, bool noLocal = false, bool noAck = false, bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            return new ConsumerConf(queueName, consumerTag, noLocal, noAck, exclusive, false, arguments);
        }
        public static ConsumerConf Create(string queueName, string consumerTag, bool noAck = false)
        {
            return new ConsumerConf(queueName, consumerTag, false, noAck, false, false, null);
        }
    }
}
