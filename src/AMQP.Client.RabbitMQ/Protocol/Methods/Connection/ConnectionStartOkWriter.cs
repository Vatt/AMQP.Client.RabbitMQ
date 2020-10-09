using System;
using System.Buffers;
using System.Buffers.Binary;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Internal;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    internal class ConnectionStartOkWriter : IMessageWriter<ClientConf>
    {
        private readonly ConnectionConf _info;
        public ConnectionStartOkWriter(ConnectionConf info)
        {
            _info = info;
        }
        public void WriteMessage(ClientConf message, IBufferWriter<byte> output)
        {
            var writer = new ValueWriter(output);
            writer.WriteOctet(1);
            writer.WriteShortInt(0);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(10, 11, ref writer);
            writer.WriteTable(message.Properties);
            writer.WriteShortStr(message.Mechanism);
            writer.WriteLongStr($"\0{_info.User}\0{_info.Password}");
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
