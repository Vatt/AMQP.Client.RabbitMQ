using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Framing
{

    internal enum PropertiesTag : byte
    {
        ContentType,
        ContentEncoding,
        Headers,
        DeliveryMode,
        Priority,
        CorrelationId,
        ReplyTo,
        Expiration,
        MessageId,
        Timestamp,
        Type,
        UserId,
        AppId,
        ClusterId,
    }
    public struct ContentHeaderProperties
    {
        private Dictionary<PropertiesTag, object> _properties;

        public string ContentType
        {
            get => _properties.GetValueOrDefault(PropertiesTag.ContentType) as string;
            set => _properties[PropertiesTag.ContentType] = value;
        }


        public string ContentEncoding
        {
            get => _properties.GetValueOrDefault(PropertiesTag.ContentEncoding) as string;
            set => _properties[PropertiesTag.ContentEncoding] = value;
        }


        public Dictionary<string, object> Headers
        {
            get => _properties.GetValueOrDefault(PropertiesTag.Headers) as Dictionary<string, object>;
            set => _properties[PropertiesTag.Headers] = value;
        }

        public byte DeliveryMode
        {
            get
            {
                if (!_properties.TryGetValue(PropertiesTag.DeliveryMode, out var value))
                {
                    return 0;
                }
                return (byte)value;
            }
            set => _properties[PropertiesTag.DeliveryMode] = value;
        }



        public byte Priority
        {
            get
            {
                if (!_properties.TryGetValue(PropertiesTag.Priority, out var value))
                {
                    return 0;
                }
                return (byte)value;
            }
            set => _properties[PropertiesTag.Priority] = value;
        }

        public string CorrelationId
        {
            get => _properties.GetValueOrDefault(PropertiesTag.CorrelationId) as string;
            set => _properties[PropertiesTag.CorrelationId] = value;
        }

        public string ReplyTo
        {
            get => _properties.GetValueOrDefault(PropertiesTag.ReplyTo) as string;
            set => _properties[PropertiesTag.ReplyTo] = value;
        }

        public string Expiration
        {
            get => _properties.GetValueOrDefault(PropertiesTag.Expiration) as string;
            set => _properties[PropertiesTag.Expiration] = value;
        }

        public string MessageId
        {
            get => _properties.GetValueOrDefault(PropertiesTag.MessageId) as string;
            set => _properties[PropertiesTag.MessageId] = value;
        }
        public long Timestamp
        {
            get
            {
                if (!_properties.TryGetValue(PropertiesTag.Timestamp, out var value))
                {
                    return 0;
                }
                return (long)value;
            }
            set => _properties[PropertiesTag.Timestamp] = value;
        }
        public string Type
        {
            get => _properties.GetValueOrDefault(PropertiesTag.Type) as string;
            set => _properties[PropertiesTag.Type] = value;
        }
        public string UserId
        {
            get => _properties.GetValueOrDefault(PropertiesTag.UserId) as string;
            set => _properties[PropertiesTag.UserId] = value;
        }
        public string AppId
        {
            get => _properties.GetValueOrDefault(PropertiesTag.AppId) as string;
            set => _properties[PropertiesTag.AppId] = value;
        }
        public string ClusterId
        {
            get => _properties.GetValueOrDefault(PropertiesTag.ClusterId) as string;
            set => _properties[PropertiesTag.ClusterId] = value;
        }

        public static ContentHeaderProperties Default() => new ContentHeaderProperties { _properties = new Dictionary<PropertiesTag, object>() };

    }
    public struct ContentHeader
    {
        public readonly ushort ClassId;
        public readonly ushort Weight;
        public readonly long BodySize;
        public ContentHeaderProperties Properties;
        public ContentHeader(ushort classId, ushort weight, long bodySize) : this(classId, bodySize)
        {
            Weight = weight;
            Properties = ContentHeaderProperties.Default();
        }
        public ContentHeader(ushort classId, long bodySize)
        {
            ClassId = classId;
            Weight = 0;
            BodySize = bodySize;
            Properties = ContentHeaderProperties.Default();
        }

        public ContentHeader(ushort classId, long bodySize, ref ContentHeaderProperties properties)
        {
            ClassId = classId;
            Weight = 0;
            BodySize = bodySize;
            Properties = properties;
        }
    }
}
