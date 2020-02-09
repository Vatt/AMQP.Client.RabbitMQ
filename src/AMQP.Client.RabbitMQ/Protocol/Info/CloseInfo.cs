using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Info
{
    public readonly struct CloseInfo
    {
        public readonly short ChannelId;
        public readonly short ReplyCode;
        public readonly string ReplyText;
        public readonly short FailedClassId;
        public readonly short FailedMethodId;
        public CloseInfo(short channelId, short code, string text, short failedClassId, short failedMethodId)
        {
            ChannelId = channelId;
            ReplyCode = code;
            ReplyText = text;
            FailedClassId = failedClassId;
            FailedMethodId = failedMethodId;
        }
    }
}
