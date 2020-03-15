﻿using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    /* 
     *               Tune method frame
     * 
     * 0             2           6           8
     * +-------------+-----------+-----------+
     * | channel-max | frame-max | heartbeat |
     * +-------------+-----------+-----------+
     */
    internal class ConnectionTuneReader : IMessageReader<RabbitMQMainInfo>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out RabbitMQMainInfo message)
        {
            message = default;
            ValueReader reader = new ValueReader(input);
            if (!reader.ReadShortInt(out ushort chanellMax)) { return false; }
            if (!reader.ReadLong(out var frameMax)) { return false; }
            if (!reader.ReadShortInt(out short heartbeat)) { return false; }
            if (!reader.ReadOctet(out var end_frame_marker)) { return false; }
            if (end_frame_marker != 206)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            message = new RabbitMQMainInfo(chanellMax, frameMax, heartbeat);
            consumed = reader.Position;
            examined = reader.Position;
            return true;
        }
    }
}