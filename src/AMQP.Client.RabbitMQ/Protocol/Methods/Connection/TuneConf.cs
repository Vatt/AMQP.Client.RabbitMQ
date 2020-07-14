namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    public struct TuneConf
    {
        public ushort ChannelMax;
        public int FrameMax;
        public short Heartbeat;
        public TuneConf(ushort chanellMax, int frameMax, short heartbeat)
        {
            ChannelMax = chanellMax;
            FrameMax = frameMax;
            Heartbeat = heartbeat;
        }
        public static TuneConf DefaultConnectionInfo()
        {
            return new TuneConf(2047, 131072, 60);
            //return new TuneConf(2047, 131072 / 4, 60);
        }

    }
}
