using System;
using System.Buffers;
using System.Buffers.Binary;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Internal;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    internal class ConnectionStartOkWriter : IMessageWriter<(ConnectionConf, ClientConf)>
    {
        public void WriteMessage((ConnectionConf, ClientConf) messagePair, IBufferWriter<byte> output)
        {
            (var info, var  message) = messagePair;
            var writer = new ValueWriter(output);
            writer.WriteOctet(1);
            writer.WriteShortInt(0);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(10, 11, ref writer);
            writer.WriteTable(message.Properties);
            writer.WriteShortStr(message.Mechanism);
            writer.WriteLongStr($"\0{info.User}\0{info.Password}");
            writer.WriteShortStr(message.Locale);
            var paylodaSize = writer.Written - checkpoint;
            writer.WriteOctet(206);

            Span<byte> bytes = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(bytes, paylodaSize);
            reserved.Write(bytes);
            writer.Commit();
        }
    }
}
