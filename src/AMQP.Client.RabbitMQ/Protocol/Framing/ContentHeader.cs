using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Framing
{
    
    internal enum PropertiesTag:byte
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
        public string ContentType()
        {
            return _properties.GetValueOrDefault(PropertiesTag.ContentType) as string;
        }
        public void ContentType(string value)
        {
            _properties[PropertiesTag.ContentType] = value;
        }
        public string ContentEncoding()
        {
            return _properties.GetValueOrDefault(PropertiesTag.ContentEncoding) as string;
        }
        public void ContentEncoding(string value)
        {
            _properties[PropertiesTag.ContentEncoding] = value;
        }
        public Dictionary<string, object> Headers()
        {
            return _properties.GetValueOrDefault(PropertiesTag.Headers) as Dictionary<string, object>;
        }
        public void Headers(Dictionary<string, object> value)
        {
            _properties[PropertiesTag.Headers] = value;
        }
        public byte DeliveryMode()
        {
            if(!_properties.TryGetValue(PropertiesTag.DeliveryMode,out var value))
            {
                return 0;
            }
            return (byte)value;
        }
        public void DeliveryMode(byte value)
        {
            _properties[PropertiesTag.DeliveryMode] = value;
        }
        public byte Priority()
        {
            if (!_properties.TryGetValue(PropertiesTag.Priority, out var value))
            {
                return 0;
            }
            return (byte)value;
        }
        public void Priority(byte value)
        {
            _properties[PropertiesTag.Priority] = value;
        }
        public string CorrelationId()
        {
            return _properties.GetValueOrDefault(PropertiesTag.CorrelationId) as string;
        }
        public void CorrelationId(string value)
        {
            _properties[PropertiesTag.CorrelationId] = value;
        }
        public string ReplyTo()
        {
            return _properties.GetValueOrDefault(PropertiesTag.ReplyTo) as string;
        }
        public void ReplyTo(string value)
        {
            _properties[PropertiesTag.ReplyTo] = value;
        }
        public string Expiration()
        {
            return _properties.GetValueOrDefault(PropertiesTag.Expiration) as string;
        }
        public void Expiration(string value)
        {
            _properties[PropertiesTag.Expiration] = value;
        }
        public string MessageId()
        {
            return _properties.GetValueOrDefault(PropertiesTag.MessageId) as string;
        }
        public void MessageId(string value)
        {
            _properties[PropertiesTag.MessageId] = value;
        }
        public long Timestamp()
        {
            if (!_properties.TryGetValue(PropertiesTag.Timestamp, out var value))
            {
                return 0;
            }
            return (long)value;
        }
        public void Timestamp(long value)
        {
            _properties[PropertiesTag.Timestamp] = value;
        }
        public string Type()
        {
            return _properties.GetValueOrDefault(PropertiesTag.Type) as string;
        }
        public void Type(string value)
        {
            _properties[PropertiesTag.Type] = value;
        }
        public string UserId()
        {
            return _properties.GetValueOrDefault(PropertiesTag.UserId) as string;
        }
        public void UserId(string value)
        {
            _properties[PropertiesTag.UserId] = value;
        }
        public string AppId()
        {
            return _properties.GetValueOrDefault(PropertiesTag.AppId) as string;
        }
        public void AppId(string value)
        {
            _properties[PropertiesTag.AppId] = value;
        }
        public string ClusterId()
        {
            return _properties.GetValueOrDefault(PropertiesTag.ClusterId) as string;
        }
        public void ClusterId(string value)
        {
            _properties[PropertiesTag.ClusterId] = value;
        }
        
        public static ContentHeaderProperties Default() => new ContentHeaderProperties { _properties = new Dictionary<PropertiesTag, object>() };
        
    }
    public struct ContentHeader
    {
        public readonly ushort ClassId;
        public readonly ushort Weight;
        public readonly long BodySize;
        public ContentHeaderProperties Properties;
        public ContentHeader(ushort classId, ushort weight, long bodySize):this(classId,bodySize)
        {
            Weight = weight;
            Properties = ContentHeaderProperties.Default();
        }
        public ContentHeader(ushort classId, long bodySize)
        {
            ClassId = classId;
            Weight = 0;
            BodySize = bodySize;
            Properties  = ContentHeaderProperties.Default();
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
