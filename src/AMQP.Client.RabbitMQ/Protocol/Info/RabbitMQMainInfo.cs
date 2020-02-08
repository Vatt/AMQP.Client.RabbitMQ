namespace AMQP.Client.RabbitMQ.Protocol.Info
{
    public struct RabbitMQMainInfo
    {
        public short ChannelMax;
        public int FrameMax;
        public short Heartbeat;
        public RabbitMQMainInfo(short chanellMax,int frameMax, short heartbeat)
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
