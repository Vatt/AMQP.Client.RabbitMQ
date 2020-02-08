using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    internal class RabbitMQChannelHandler : IRabbitMQFrameHandler
    {
        private static short _channelId = 0;
        public Dictionary<short, RabbitMQChannelBase> _channels;
        public RabbitMQChannel0 Channel0;
        private RabbitMQProtocol _protocol;
        public RabbitMQChannelHandler(RabbitMQProtocol protocol)
        {
            _channels = new Dictionary<short, RabbitMQChannelBase>();
            _protocol = protocol;
        }
        public async ValueTask HandleFrameAsync(FrameHeader header)
        {
            if (header.Chanell == 0)
            {
                await Channel0.HandleFrameAsync(header);
                return;
            }
            if(!_channels.TryGetValue(header.Chanell,out RabbitMQChannelBase channel))
            {
                throw new Exception($"{nameof(RabbitMQChannelHandler): channel-id missmatch}");
            }
            await channel.HandleFrameAsync(header);
        }
        public async ValueTask<RabbitMQChannelBase> CreateChannel()
        {
            var id = ++_channelId;
            return default;
        }
    }
}
