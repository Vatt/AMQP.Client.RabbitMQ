namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    public readonly struct CloseInfo
    {
        public readonly ushort ChannelId;
        public readonly short ClassId;
        public readonly short MethodId;
        public readonly short ReplyCode;
        public readonly string ReplyText;
        public readonly short FailedClassId;
        public readonly short FailedMethodId;
        public CloseInfo(ushort channel, short classId, short methodId, short code, string text, short failedClassId, short failedMethodId)
        {
            ChannelId = channel;
            ClassId = classId;
            MethodId = methodId;
            ReplyCode = code;
            ReplyText = text;
            FailedClassId = failedClassId;
            FailedMethodId = failedMethodId;
        }

        public static CloseInfo Create(short code, string text, short failedClassId, short failedMethodId)
        {
            return new CloseInfo(0,0,0, code, text, failedClassId, failedMethodId);
        }
    }
}
