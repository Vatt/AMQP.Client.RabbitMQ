﻿using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    public readonly struct NoPaylodMethodInfo
    {
        public readonly byte FrameType;
        public readonly ushort Channel;
        public readonly short ClassId;
        public readonly short MethodId;
        public NoPaylodMethodInfo(byte type, ushort channel, short classid, short methodid)
        {
            FrameType = type;
            Channel = channel;
            ClassId = classid;
            MethodId = methodid;
        }
    }

    public class NoPayloadMethodWrtier : IMessageWriter<NoPaylodMethodInfo>
    {
        public void WriteMessage(NoPaylodMethodInfo message, IBufferWriter<byte> output)
        {
            var writer = new ValueWriter(output);
            FrameWriter.WriteFrameHeader(message.FrameType, message.Channel, 4, ref writer);
            FrameWriter.WriteMethodFrame(message.ClassId, message.MethodId, ref writer);
            writer.WriteOctet(RabbitMQConstants.FrameEnd);
            writer.Commit();
        }
    }
}
