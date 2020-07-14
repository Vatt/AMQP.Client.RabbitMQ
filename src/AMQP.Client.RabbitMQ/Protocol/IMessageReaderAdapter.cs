using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol
{
    internal interface IMessageReaderAdapter<T>
    {
        bool TryParseMessage(in ReadOnlySequence<byte> input, out T message);
    }
}