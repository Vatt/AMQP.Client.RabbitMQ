using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    internal class RabbitMQChannelHandler
    {
        private static ushort _channelId = 0; //Interlocked?
        private readonly ConcurrentDictionary<ushort, RabbitMQDefaultChannel> _channels;
        private RabbitMQProtocol _protocol;
        //        private ushort _maxChannels;
        private RabbitMQChannelZero _channel0; 
        public RabbitMQChannelHandler(RabbitMQProtocol protocol, RabbitMQChannelZero channel0)
        {
            _channels = new ConcurrentDictionary<ushort, RabbitMQDefaultChannel>();
            _protocol = protocol;
            _channel0 = channel0;
        }
        public async ValueTask HandleFrameAsync(FrameHeader header)
        {
            if(!_channels.TryGetValue(header.Channel,out RabbitMQDefaultChannel channel))
            {
                throw new Exception($"{nameof(RabbitMQChannelHandler)}: channel-id({header.Channel}) missmatch");
            }
            await channel.HandleAsync(header);
        }
        public async ValueTask<IRabbitMQDefaultChannel> CreateChannel()
        {
            var id = ++_channelId;
            if (id > _channel0.MainInfo.ChannelMax)
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
        private void CloseChannelPrivate(ushort id)
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
