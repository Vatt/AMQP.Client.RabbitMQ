using AMQP.Client.RabbitMQ.Protocol.Info;
using System;

namespace AMQP.Client.RabbitMQ.Encoder
{
    internal class FrameEncoder
    {
        public static int EncodeStartOkFrame(Memory<byte> destination, RabbitMQClientInfo clientInfo, RabbitMQConnectionInfo connInfo)
        {
            ValueWriter1234567890 encoder = new ValueWriter1234567890(destination);
            int payloadSize = 0;
            encoder.SetPosition(7);
            payloadSize += EncodeMethodFrame(10, 11, ref encoder);
            payloadSize += encoder.WriteTable(clientInfo.Properties);
            payloadSize += encoder.WriteShortStr(clientInfo.Mechanism);
            payloadSize += encoder.WriteLongStr($"\0{connInfo.User}\0{connInfo.Password}");
            payloadSize += encoder.WriteShortStr(clientInfo.Locale);
            encoder.WriteOctet(206);
            encoder.Reset();
            EncodeFrameHeader(1, 0, payloadSize , ref encoder);
            return 7 + payloadSize + 1;
        }
        public static int EncodeTuneOKFrame(Memory<byte> destination, RabbitMQInfo connectionInfo)
        {
            ValueWriter1234567890 encoder = new ValueWriter1234567890(destination);
            EncodeFrameHeader(1, 0, 12, ref encoder);
            EncodeMethodFrame(10, 31, ref encoder);
            encoder.WriteShortInt(connectionInfo.ChanellMax);
            encoder.WriteLong(connectionInfo.FrameMax);
            encoder.WriteShortInt(connectionInfo.Heartbeat);
            encoder.WriteOctet(206);
            return 20;
        }
        public static int EncodeOpenFrame(Memory<byte> destination, string vhost)
        {
            ValueWriter1234567890 encoder = new ValueWriter1234567890(destination);
            EncodeFrameHeader(1, 0, 8, ref encoder);
            var payloadSize = EncodeMethodFrame(10, 40, ref encoder);
            payloadSize += encoder.WriteShortStr(vhost);
            payloadSize += encoder.WriteOctet(0);
            payloadSize += encoder.WriteOctet(0);
            encoder.WriteOctet(206);
            return 7 + payloadSize + 1;
        }
        public static int EncodeFrameHeader(byte type, short chanell, int payloadSize, ref ValueWriter1234567890 encoder)
        {
            encoder.WriteOctet(type);
            encoder.WriteShortInt(chanell);
            encoder.WriteLong(payloadSize);
            return 8;
        }
        public static int EncodeMethodFrame(short classId,short methodId,ref ValueWriter1234567890 encoder)
        {
            encoder.WriteShortInt(classId);             
            encoder.WriteShortInt(methodId);             
            return 4;
        }
    }
}
