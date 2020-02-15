using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Framing
{
    public struct ContentHeader
    {
        public readonly ushort ClassId;
        public readonly ushort Weight;
        public readonly long BodySize;
        public string ContentType;
        public string ContentEncoding;
        public Dictionary<string, object> Headers;
        public byte DeliveryMode;
        public byte Priority;
        public string CorrelationId;
        public string ReplyTo;
        public string Expiration;
        public string MessageId;
        public long Timestamp;
        public string Type;
        public string UserId;
        public string AppId;
        public string ClusterId;
        public ContentHeader(ushort classId,ushort weight, long bodySize)
        {
            ClassId = classId;
            Weight = weight;
            BodySize = bodySize;
            ContentType = default;
            ContentEncoding = default;
            Headers = null;
            DeliveryMode = default;
            Priority = default;
            CorrelationId = default;
            ReplyTo = default;
            Expiration = default;
            MessageId = default;
            Timestamp = default;
            Type = default;
            UserId = default;
            AppId = default;
            ClusterId = default;
    }
    }
}
