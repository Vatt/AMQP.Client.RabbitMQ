using System;

namespace AMQP.Client.Abstractions
{
    public struct AMQPApiVersion
    {
        public readonly int Major;
        public readonly int Minor;
        public readonly int Revision;
        public AMQPApiVersion(int major, int minor, int revision)
        {
            Major = major;
            Minor = minor;
            Revision = revision;
        }
    }
}
