namespace AMQP.Client.RabbitMQ.Protocol.Framing
{
    public readonly struct MethodHeader
    {
        public readonly short ClassId;
        public readonly short MethodId;

        public MethodHeader(short classId, short methodId)
        {
            ClassId = classId;
            MethodId = methodId;
        }
    }
}