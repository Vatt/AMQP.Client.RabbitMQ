namespace AMQP.Client.RabbitMQ.Protocol.Methods.Common
{
    public readonly struct CloseInfo
    {
        public readonly short ReplyCode;
        public readonly string ReplyText;
        public readonly short FailedClassId;
        public readonly short FailedMethodId;
        public CloseInfo(short code, string text, short failedClassId, short failedMethodId)
        {
            ReplyCode = code;
            ReplyText = text;
            FailedClassId = failedClassId;
            FailedMethodId = failedMethodId;
        }
    }
}
