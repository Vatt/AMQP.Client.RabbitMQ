namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    public struct RabbitMQMainInfo
    {
        public ushort ChannelMax;
        public int FrameMax;
        public short Heartbeat;
        public RabbitMQMainInfo(ushort chanellMax, int frameMax, short heartbeat)
        {
            ChannelMax = chanellMax;
            FrameMax = frameMax;
            Heartbeat = heartbeat;
        }
        public static RabbitMQMainInfo DefaultConnectionInfo()
        {
            return new RabbitMQMainInfo(2047, 131072, 60);
        }

    }
}
