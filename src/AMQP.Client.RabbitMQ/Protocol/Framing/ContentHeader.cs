using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Framing
{
    public struct ContentHeaderProperties
    {
        public string ContentType;
        public string ContentEncoding;
        public Dictionary<string, object> Headers;
        public string CorrelationId;
        public string ReplyTo;
        public string Expiration;
        public string MessageId;
        public long Timestamp;
        public string Type;
        public string UserId;
        public string AppId;
        public string ClusterId;
        public byte DeliveryMode;

        public byte Priority;
        //public ContentHeaderProperties()
        //{
        //    ContentType = default;
        //    ContentEncoding = default;
        //    Headers = null;
        //    DeliveryMode = default;
        //    Priority = default;
        //    CorrelationId = default;
        //    ReplyTo = default;
        //    Expiration = default;
        //    MessageId = default;
        //    Timestamp = default;
        //    Type = default;
        //    UserId = default;
        //    AppId = default;
        //    ClusterId = default;
        //}
    }

    public class ContentHeader
    {
        public readonly ushort ChannelId;
        public readonly long BodySize;
        public ContentHeaderProperties Properties;
        public readonly ushort ClassId;
        public readonly ushort Weight;
        public ContentHeader(ushort channelId, ushort classId, long bodySize)
        {
            ChannelId = channelId;
            ClassId = classId;
            Weight = 0;
            BodySize = bodySize;
            Properties = new ContentHeaderProperties();
        }

        public ContentHeader(ushort channelId, ushort classId, long bodySize, ref ContentHeaderProperties properties)
        {
            ChannelId = channelId;
            ClassId = classId;
            Weight = 0;
            BodySize = bodySize;
            Properties = properties;
        }
    }
}