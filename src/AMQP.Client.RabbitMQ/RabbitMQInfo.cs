using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ
{
    public readonly struct RabbitMQInfo
    {
        public readonly short ChanellMax;
        public readonly int FrameMax;
        public readonly short Heartbeat;
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
