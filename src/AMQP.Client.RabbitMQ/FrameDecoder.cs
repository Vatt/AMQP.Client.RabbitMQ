using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AMQP.Client.RabbitMQ
{
    internal ref struct FrameHeader
    {
        public int FrameType;
        public int Chanell;
        public int PaylodaSize;
        public FrameHeader(int type,int chanell,int payloadSize)
        {
            FrameType = type;
            Chanell = chanell;
            PaylodaSize = payloadSize;
        }
    }
    internal class FrameDecoder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ServerInfo DecodeStartMethodFrame(ReadOnlySpan<byte> frameSpan)
        {
            ValueDecoder decoder = new ValueDecoder(frameSpan);
            var header = DecodeFrameHeader(ref decoder);
            var classId = decoder.ReadShortInt();
            var methodId = decoder.ReadShortInt();
            if (header.FrameType != 1 && header.Chanell != 0 && classId != 10 && methodId != 10)
            {
                throw new Exception("FrameDecoder: start method decode failed");
            }
            var major = decoder.ReadOctet();
            var minor = decoder.ReadOctet();
            var tab = decoder.ReadTable();
            var mechanisms = decoder.ReadLongStr();
            var locales = decoder.ReadLongStr();

            return new ServerInfo(major,minor,tab,mechanisms,locales);
        }


        public static void DecodeFrame(ReadOnlySpan<byte> frameSpan)
        {
            ValueDecoder decoder = new ValueDecoder(frameSpan);
            var header = DecodeFrameHeader(ref decoder);
            switch (header.FrameType)
            {
                case 1: break;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FrameHeader DecodeFrameHeader(ref ValueDecoder decoder)
        {
            int frameType = decoder.ReadOctet();
            int chanell = decoder.ReadShortInt();
            int payloadSize = decoder.ReadLong();
            int endMarker = decoder.Data[decoder.Position + payloadSize];
            if (endMarker != 206)
            {
                throw new Exception("FrameDecoder: end-marker missmatch");
            }
            return new FrameHeader(frameType, chanell, payloadSize);
        }
    }
}
