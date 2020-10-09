﻿using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    /* 
     *                                   Frame Structure
     *                                   
    0 byte 1  short  3        int       7                          PayloadSize+7    PayloadSize+8
    +------+---------+------------------+-------------------------------+--------------+
    | type | chanell |    PayloadSize   |            Payload            |  end-marker  |
    +------+---------+------------------+-------------------------------+--------------+
                                          ContetnFrame,MethodFrame,      const value 206
                                          BodyFrame,Heartbeatframe,etc.                               
    */
    public class FrameHeaderReader : IMessageReader<FrameHeader>, IMessageReaderAdapter<FrameHeader>
    {
        public FrameHeaderReader()
        {
        }
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out FrameHeader message)
        {
            //SequenceReader<byte> reader = new SequenceReader<byte>(input);
            //message = default;
            ////Frame header = byte + short + int
            //if (reader.Remaining < 7)
            //{
            //    return false;
            //}
            //reader.TryRead(out byte type);
            //reader.TryReadBigEndian(out short channel);
            //reader.TryReadBigEndian(out int payloadSize);
            //message = new FrameHeader(type, channel, payloadSize);

            //consumed = reader.Position;
            //examined = consumed;
            ValueReader reader = new ValueReader(input, consumed);
            message = default;
            if (input.Length < 7)
            {
                return false;
            }
            reader.ReadOctet(out byte type);
            reader.ReadShortInt(out short channel);
            reader.ReadLong(out int payloadSize);
            message = new FrameHeader(type, (ushort)channel, payloadSize);

            consumed = reader.Position;
            examined = consumed;
            return true;

        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, out FrameHeader message)
        {
            ValueReader reader = new ValueReader(input);
            message = default;
            if (input.Length < 7)
            {
                return false;
            }
            reader.ReadOctet(out byte type);
            reader.ReadShortInt(out short channel);
            reader.ReadLong(out int payloadSize);
            message = new FrameHeader(type, (ushort)channel, payloadSize);
            return true;
        }
    }

}
