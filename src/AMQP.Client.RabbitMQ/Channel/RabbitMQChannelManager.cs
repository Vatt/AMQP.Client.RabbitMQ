using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    internal class RabbitMQChannelManager
    {
        private static short _channelId = 0; //Interlocked?
        private readonly Dictionary<short, IRabbitMQChannel> _channels;
        private RabbitMQProtocol _protocol;
        public RabbitMQChannelManager(RabbitMQProtocol protocol, short maxChannels)
        {
            _channels = new Dictionary<short, IRabbitMQChannel>();
            _protocol = protocol;
        }
        public async ValueTask HandleFrameAsync(FrameHeader header)
        {
            if(!_channels.TryGetValue(header.Chanell,out IRabbitMQChannel channel))
            {
                throw new Exception($"{nameof(RabbitMQChannelManager)}: channel-id({header.Chanell}) missmatch");
            }
            await channel.HandleAsync(header);
        }
        public async ValueTask<IRabbitMQChannel> CreateChannel()
        {
            var id = ++_channelId;
            var channel = new RabbitMQChannel(_protocol,id);
            _channels[id] = channel;
            var openned = await channel.TryOpenChannelAsync();
            if(!openned)
            {
                _channels.Remove(id);
                return default;
            }
            return channel;
        }
    }
}
