﻿using System;
using System.Buffers;
using System.Buffers.Binary;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Internal;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    internal class QueuePurgeWriter : IMessageWriter<QueuePurge>
    {
        private readonly ushort _channelId;
        public QueuePurgeWriter(ushort channelId)
        {
            _channelId = channelId;
        }
        public void WriteMessage(QueuePurge message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            writer.WriteOctet(RabbitMQConstants.FrameMethod);
            writer.WriteShortInt(_channelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(50, 30, ref writer);
            writer.WriteShortInt(0); //reserved-1
            writer.WriteShortStr(message.Name);
            writer.WriteBit(message.NoWait);
            writer.BitFlush();
            var payloadSize = writer.Written - checkpoint;
            writer.WriteOctet(RabbitMQConstants.FrameEnd);

            Span<byte> span = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(span, payloadSize);
            reserved.Write(span);

            writer.Commit();
        }
    }
}
