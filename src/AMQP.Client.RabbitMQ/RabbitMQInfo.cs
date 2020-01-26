using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ
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
            return new RabbitMQInfo(0, 131072, 60);
        }

    }
}
