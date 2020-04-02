using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQListener
    {
        private IConnectionHandler _connectionHandler;
        private IChannelHandler _channelHandler;
        CancellationToken _token;
        public async Task StartAsync(RabbitMQProtocolReader reader, IConnectionHandler connection, IChannelHandler channel, CancellationToken token = default)
        {
            _connectionHandler = connection;
            _channelHandler = channel;
            _token = token;
            while (true)
            {
                var frame = await reader.ReadAsync(new FrameReader(), token).ConfigureAwait(false);
                if (frame.Header.Channel == 0)
                {
                    await ProcessConnection(reader, frame);
                }
                reader.Advance();
            }
        }
        public ValueTask ProcessConnection(RabbitMQProtocolReader protocol, Frame frame)
        {
            var method = protocol.ReadMethodHeader(frame.Payload);
            var payload = frame.Payload.Slice(4);// MethodHeader size
            switch (method.MethodId)
            {
                case 10:
                    {
                        return _connectionHandler.OnStartAsync(protocol.ReadStart(payload));
                    }

                case 30:
                    {
                        return _connectionHandler.OnTuneAsync(protocol.ReadTuneMethod(payload));
                    }
                case 41:
                    {
                        var result = protocol.ReadConnectionOpenOk(payload);
                        return _connectionHandler.OnOpenOkAsync();
                    }
                case 50: //close
                    {
                        return _connectionHandler.OnCloseAsync(protocol.ReadClose(payload));
                    }
                case 51://close-ok
                    {
                        var result = protocol.ReadCloseOk(payload);
                        return _connectionHandler.OnCloseOkAsync();
                    }

                default:
                    throw new Exception($"{nameof(RabbitMQListener)}.{nameof(ProcessConnection)} :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }

        public ValueTask ProcessChannel(RabbitMQProtocolReader protocol, Frame frame)
        {
            var method = protocol.ReadMethodHeader(frame.Payload);
            var payload = frame.Payload.Slice(4);// MethodHeader size
            switch (method.MethodId)
            {
                case 11://open-ok
                    {
                        return _channelHandler.OnChannelOpenOkAsync(frame.Header.Channel);
                    }
                case 40: //close
                    {
                        return _channelHandler.OnChannelCloseAsync(frame.Header.Channel, protocol.ReadClose(payload));

                    }
                case 41://close-ok
                    {
                        return _channelHandler.OnChannelCloseOkAsync(frame.Header.Channel);
                    }
                default:
                    throw new Exception($"{nameof(RabbitMQListener)}.{nameof(ProcessChannel)} :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
    }
}
