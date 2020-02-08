using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    internal class RabbitMQChannelsHandler
    {
        private static short _channelId = 0; //Interlocked?
        private readonly Dictionary<short, IRabbitMQChannel> _channels;
        private RabbitMQProtocol _protocol;
        public RabbitMQChannelsHandler(RabbitMQProtocol protocol, short maxChannels)
        {
            _channels = new Dictionary<short, IRabbitMQChannel>();
            _protocol = protocol;
        }
        public async ValueTask HandleFrameAsync(FrameHeader header)
        {
            if(!_channels.TryGetValue(header.Chanell,out IRabbitMQChannel channel))
            {
                throw new Exception($"{nameof(RabbitMQChannelsHandler): channel-id missmatch}");
            }
            await channel.HandleAsync(header);
        }
        public async ValueTask<IRabbitMQChannel> CreateChannel()
        {
            var id = ++_channelId;
            return default;
        }
    }
}
