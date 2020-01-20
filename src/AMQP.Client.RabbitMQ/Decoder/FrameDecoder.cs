using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AMQP.Client.RabbitMQ.Decoder
{
    internal ref struct FrameHeader
    {
        public readonly byte FrameType;
        public readonly short Chanell;
        public readonly int PaylodaSize;
        public FrameHeader(byte type,short chanell,int payloadSize)
        {
            FrameType = type;
            Chanell = chanell;
            PaylodaSize = payloadSize;
        }
    }
    internal class FrameDecoder
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SequencePosition DecodeStartMethodFrame(ReadOnlySequence<byte> sequence, out RabbitMQServerInfo info)
        {
            ValueDecoder decoder = new ValueDecoder(sequence);
            var header = DecodeFrameHeader(ref decoder);
            var classId = decoder.ReadShortInt();
            var methodId = decoder.ReadShortInt();
            if (header.FrameType != 1 && header.Chanell != 0 && classId != 10 && methodId != 10)
            {
                DecoderThrowHelper.ThrowFrameDecoderStartMethodDecodeFailed();
            }
            var major = decoder.ReadOctet();
            var minor = decoder.ReadOctet();
            var tab = decoder.ReadTable();
            var mechanisms = decoder.ReadLongStr();
            var locales = decoder.ReadLongStr();
            var end_frame_marker = decoder.ReadOctet();
            if (end_frame_marker != 206)  
            {
                DecoderThrowHelper.ThrowFrameDecoderEndMarkerMissmatch();
            }
            info =  new RabbitMQServerInfo(major,minor,tab,mechanisms,locales);
            return decoder.reader.Position;
        }


        public static void DecodeFrame(ReadOnlySequence<byte> sequence)
        {
            ValueDecoder decoder = new ValueDecoder(sequence);
            var header = DecodeFrameHeader(ref decoder);
            switch (header.FrameType)
            {
                case 1: break;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FrameHeader DecodeFrameHeader(ref ValueDecoder decoder)
        {
            byte frameType = decoder.ReadOctet();
            short chanell = decoder.ReadShortInt();
            int payloadSize = decoder.ReadLong();

            return new FrameHeader(frameType, chanell, payloadSize);
        }
    }
}
