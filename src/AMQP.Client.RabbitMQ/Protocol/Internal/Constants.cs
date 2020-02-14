namespace AMQP.Client.RabbitMQ.Protocol.Internal
{
    internal sealed class Constants
    {
 
        public const byte FrameMethod = 1;
        public const byte FrameHeader = 2;
        public const byte FrameBody = 3;
        public const byte FrameHeartbeat = 8;
        public const int FrameMinSize = 4096;
        public const byte FrameEnd = 206;
        public const int ReplySuccess = 200;
        public const int ContentTooLarge = 311;
        public const int NoConsumers = 313;
        public const int ConnectionForced = 320;
        public const int InvalidPath = 402;
        public const int AccessRefused = 403;
        public const int NotFound = 404;
        public const int ResourceLocked = 405;
        public const int PreconditionFailed = 406;
        public const int FrameError = 501;
        public const int SyntaxError = 502;
        public const int CommandInvalid = 503;
        public const int ChannelError = 504;
        public const int UnexpectedFrame = 505;
        public const int ResourceError = 506;
        public const int NotAllowed = 530;
        public const int NotImplemented = 540;
        public const int InternalError = 541;
    }
}
