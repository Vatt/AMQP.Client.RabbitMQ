﻿using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using System;
using System.Buffers;
using System.Buffers.Binary;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    internal class ExchangeDeclareWriter : IMessageWriter<ExchangeDeclare>
    {
        public void WriteMessage(ExchangeDeclare message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            writer.WriteOctet(1);
            writer.WriteShortInt(message.ChannelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(40, 10, ref writer);
            writer.WriteShortInt(0); //reseved-1
            writer.WriteShortStr(message.Name);
            writer.WriteShortStr(message.Type);

            writer.WriteBit(message.Passive);
            writer.WriteBit(message.Durable);
            writer.WriteBit(message.AutoDelete);
            writer.WriteBit(message.Internal);
            writer.WriteBit(message.NoWait);
            writer.WriteTable(message.Arguments);
            var size = writer.Written - checkpoint;
            writer.WriteOctet(RabbitMQConstants.FrameEnd);

            Span<byte> sizeSpan = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(sizeSpan, size);
            reserved.Write(sizeSpan);

            writer.Commit();
        }
    }
}
