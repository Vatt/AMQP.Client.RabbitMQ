namespace AMQP.Client.RabbitMQ.Protocol.Info
{
    public struct RabbitMQInfo
    {
        public short ChanellMax;
        public int FrameMax;
        public short Heartbeat;
        public RabbitMQInfo(short chanellMax,int frameMax, short heartbeat)
        {
            ChanellMax = chanellMax;
            FrameMax = frameMax;
            Heartbeat = heartbeat;
        }
        public static RabbitMQInfo DefaultConnectionInfo()
        {
            return new RabbitMQInfo(2047, 131072, 60);
        }

    }
}
