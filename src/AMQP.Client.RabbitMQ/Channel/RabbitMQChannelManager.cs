using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    internal class RabbitMQChannelManager
    {
        private static short _channelId = 0; //Interlocked?
        private readonly ConcurrentDictionary<short, RabbitMQDefaultChannel> _channels;
        private RabbitMQProtocol _protocol;
        private short _maxChannels;
        public RabbitMQChannelManager(RabbitMQProtocol protocol, short maxChannels)
        {
            _channels = new ConcurrentDictionary<short, RabbitMQDefaultChannel>();
            _protocol = protocol;
            _maxChannels = maxChannels;
        }
        public async ValueTask HandleFrameAsync(FrameHeader header)
        {
            if(!_channels.TryGetValue(header.Chanell,out RabbitMQDefaultChannel channel))
            {
                throw new Exception($"{nameof(RabbitMQChannelManager)}: channel-id({header.Chanell}) missmatch");
            }
            await channel.HandleAsync(header);
        }
        public async ValueTask<IRabbitMQDefaultChannel> CreateChannel()
        {
            var id = ++_channelId;
            if (id > _maxChannels)
            {
                return default;
            }
            var channel = new RabbitMQDefaultChannel(_protocol, id, CloseChannelPrivate);
            _channels[id] = channel;
            var openned = await channel.TryOpenChannelAsync();
            if(!openned)
            {
                if(!_channels.TryRemove(id,out RabbitMQDefaultChannel _))
                {
                    //TODO: сделать что нибудь
                }
                return default;
            }
            return channel;
        }
        private void CloseChannelPrivate(short id)
        {
            if (!_channels.TryRemove(id, out RabbitMQDefaultChannel channel))
            {
                //TODO: сделать что нибудь
            }
            if (channel != null && channel.IsOpen)
            {
                //TODO: сделать что нибудь
            }
        }
    }
}
