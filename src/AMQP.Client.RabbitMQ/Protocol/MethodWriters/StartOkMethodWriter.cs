using AMQP.Client.RabbitMQ.Protocol.Info;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.MethodWriters
{
    public class StartOkMethodWriter : IMessageWriter<RabbitMQClientInfo>
    {
        private readonly RabbitMQConnectionInfo _info;
        public StartOkMethodWriter(RabbitMQConnectionInfo info)
        {
            _info = info;
        }
        public void WriteMessage(RabbitMQClientInfo message, IBufferWriter<byte> output)
        {
            var writer = new ValueWriter(output);
            writer.WriteOctet(1); //frame type = 1,  method frame
            writer.WriteShortInt(0); //chanel = 0
            var reserved = writer.Reserve(4);// size of start-ok method
            var first = writer.Written;
            writer.WriteShortInt(10); // class-id, 10 is Connection class id
            writer.WriteShortInt(11); // method-id, 11 start-ok method id 
            writer.WriteTable(message.Properties);
            writer.WriteShortStr(message.Mechanism);
            writer.WriteLongStr($"\0{_info.User}\0{_info.Password}");
            writer.WriteShortStr(message.Locale);
            var paylodaSize = writer.Written - first;
            writer.WriteOctet(206);

            Span<byte> bytes = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(bytes, paylodaSize);
            reserved.Write(bytes);
            writer.Commit();
        }
    }
}
