using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public readonly struct ConsumerInfo
    {
        public readonly string QueueName;
        public readonly string ConsumerTag;
        public readonly bool NoLocal;
        public readonly bool NoAck;
        public readonly bool Exclusive;
        public readonly bool NoWait;
        public readonly Dictionary<string, object> Arguments;
        public ConsumerInfo(string queue, string tag, bool noLocal = false, bool noAck = false,
                            bool exclusive = false, bool nowait = false, Dictionary<string, object> arguments = null)
        {
            QueueName = queue;
            ConsumerTag = tag;
            NoLocal = noLocal;
            NoAck = noAck;
            Exclusive = exclusive;
            NoWait = nowait;
            Arguments = arguments;
        }
    }
}
