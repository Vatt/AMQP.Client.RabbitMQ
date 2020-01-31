using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
namespace AMQP.Client.RabbitMQ.Decoder
{

    internal class FrameDecoder
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SequencePosition DecodeStartMethodFrame(ReadOnlySequence<byte> sequence, out RabbitMQServerInfo info)
        {
            var advance = Unsafe.SizeOf<FrameHeader>() + Unsafe.SizeOf<MethodHeader>() - 1;
            ValueDecoder decoder = new ValueDecoder(sequence, advance);
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
            return decoder.Position;
        }
        public static SequencePosition DecodeTuneMethodFrame(ReadOnlySequence<byte> sequence, out RabbitMQInfo connectionInfo)
        {
            var advance = 11;
            ValueDecoder decoder = new ValueDecoder(sequence, advance);
            var chanellMax = decoder.ReadShortInt();
            var frameMax = decoder.ReadLong();
            var heartbeat = decoder.ReadShortInt();
            var end_frame_marker = decoder.ReadOctet();
            if (end_frame_marker != 206)  
            {
                DecoderThrowHelper.ThrowFrameDecoderEndMarkerMissmatch();
            }
            connectionInfo = new RabbitMQInfo(chanellMax, frameMax, heartbeat);
            return decoder.Position;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodHeader DecodeMethodFrame(ReadOnlySequence<byte> sequence)
        {
            ValueDecoder decoder = new ValueDecoder(sequence, Unsafe.SizeOf<FrameHeader>() - 1);
            return new MethodHeader(decoder.ReadShortInt(), decoder.ReadShortInt());
        }
        
        public static FrameHeader DecodeFrame(ReadOnlySequence<byte> sequence)
        {
            ValueDecoder decoder = new ValueDecoder(sequence);
            byte frameType = decoder.ReadOctet();
            short chanell = decoder.ReadShortInt();
            int dataSize = decoder.ReadLong();
            //var end_frame_marker = decoder.ReadOctet();
            return new FrameHeader(frameType, chanell, dataSize);
        }
    }
}
