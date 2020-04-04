using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System;
using System.Buffers;
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
            var frameReader = new FrameReader();
            while (true)
            {
                var frame = await reader.ReadAsync(frameReader, token).ConfigureAwait(false);
                if (frame.Header.FrameType == Constants.FrameHeartbeat)
                {
                    await _connectionHandler.OnHeartbeatAsync().ConfigureAwait(false);
                    continue;
                }
                //if (frame.Header.Channel == 0)
                //{
                //    await ProcessConnection(reader, ref frame);
                //}
                await ProcessMethod(reader, ref frame).ConfigureAwait(false);
                reader.Advance();
            }
        }
        internal ValueTask ProcessMethod(RabbitMQProtocolReader protocol, ref Frame frame)
        {
            if (frame.Header.FrameType != Constants.FrameMethod)
            {
                ThrowHelpers.ReaderThrowHelper.ThrowIfFrameTypeMissmatch();
            }
            var method = protocol.ReadMethodHeader(frame.Payload);
            var payload = frame.Payload.Slice(4);// MethodHeader size
            switch (method.ClassId)
            {
                case 10:
                    {
                        return ProcessConnection(protocol, ref frame.Header, ref method, payload);
                    }
                case 20:
                    {
                        return ProcessChannel(protocol, ref frame.Header, ref method, payload);
                    }
                case 40:
                    {
                        return ProcessExchange(protocol, ref frame.Header, ref method, payload);
                    }
                case 50:
                    {
                        return ProcessQueue(protocol, ref frame.Header, ref method, payload);
                    }

                case 60:
                    {
                        return ProcessBasic(protocol, ref frame.Header, ref method, payload);
                    }

                default:
                    {
                        throw new Exception($"{nameof(RabbitMQListener)}.{nameof(ProcessMethod)} :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");
                    }
            }
        }
        internal ValueTask ProcessConnection(RabbitMQProtocolReader protocol, ref FrameHeader header, ref MethodHeader method, in ReadOnlySequence<byte> payload)
        {
            if (header.Channel != 0)
            {
                ThrowHelpers.ReaderThrowHelper.ThrowIfFrameTypeMissmatch();
            }
            switch (method.MethodId)
            {
                case 10:
                    {
                        var serverConf = protocol.ReadStart(payload);
                        protocol.Advance();
                        return _connectionHandler.OnStartAsync(serverConf);
                    }

                case 30:
                    {
                        var tuneConf = protocol.ReadTuneMethod(payload);
                        protocol.Advance();
                        return _connectionHandler.OnTuneAsync(tuneConf);
                    }
                case 41:
                    {
                        var result = protocol.ReadConnectionOpenOk(payload);
                        protocol.Advance();
                        return _connectionHandler.OnOpenOkAsync();
                    }
                case 50: //close
                    {
                        var closeInfo = protocol.ReadClose(payload);
                        protocol.Advance();
                        return _connectionHandler.OnCloseAsync(closeInfo);
                    }
                case 51://close-ok
                    {
                        var result = protocol.ReadCloseOk(payload);
                        protocol.Advance();
                        return _connectionHandler.OnCloseOkAsync();
                    }

                default:
                    throw new Exception($"{nameof(RabbitMQListener)}.{nameof(ProcessConnection)} :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
        internal ValueTask ProcessChannel(RabbitMQProtocolReader protocol, ref FrameHeader header, ref MethodHeader method, in ReadOnlySequence<byte> payload)
        {
            switch (method.MethodId)
            {
                case 11://open-ok
                    {
                        protocol.Advance();
                        return _channelHandler.OnChannelOpenOkAsync(header.Channel);
                    }
                case 40: //close
                    {
                        var closeInfo = protocol.ReadClose(payload);
                        protocol.Advance();
                        return _channelHandler.OnChannelCloseAsync(header.Channel, closeInfo);

                    }
                case 41://close-ok
                    {
                        protocol.Advance();
                        return _channelHandler.OnChannelCloseOkAsync(header.Channel);
                    }
                default:
                    throw new Exception($"{nameof(RabbitMQListener)}.{nameof(ProcessChannel)} :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
        internal ValueTask ProcessExchange(RabbitMQProtocolReader protocol, ref FrameHeader header, ref MethodHeader method, in ReadOnlySequence<byte> payload)
        {
            switch (method.MethodId)
            {
                case 11: //declare-ok
                    {
                        var declareOk = protocol.ReadExchangeDeclareOk(payload);
                        protocol.Advance();
                        return _channelHandler.OnExchangeDeclareOkAsync(header.Channel);
                    }
                case 21://delete-ok
                    {
                        var declareOk = protocol.ReadExchangeDeleteOk(payload);
                        protocol.Advance();
                        return _channelHandler.OnExchangeDeleteOkAsync(header.Channel);
                    }
                default:
                    throw new Exception($"{nameof(RabbitMQListener)}.{nameof(ProcessExchange)} :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");
            }
        }
        internal ValueTask ProcessQueue(RabbitMQProtocolReader protocol, ref FrameHeader header, ref MethodHeader method, in ReadOnlySequence<byte> payload)
        {
            switch (method.MethodId)
            {
                case 11: //declare-ok
                    {
                        var declareOk = protocol.ReadQueueDeclareOk(payload);
                        return _channelHandler.OnQueueDeclareOkAsync(header.Channel, declareOk);
                    }
                case 21://bind-ok
                    {
                        var bindOk = protocol.ReadQueueBindOk(payload); // maybe delete this
                        protocol.Advance();
                        return _channelHandler.OnQueueBindOkAsync(header.Channel);
                    }
                case 51://unbind-ok
                    {
                        var unbindOk = protocol.ReadQueueUnbindOk(payload);// maybe delete this
                        protocol.Advance();
                        return _channelHandler.OnQueueUnbindOkAsync(header.Channel);
                    }
                case 31://purge-ok
                    {
                        var purged = protocol.ReadQueuePurgeOk(payload);
                        protocol.Advance();
                        return _channelHandler.OnQueuePurgeOkAsync(header.Channel, purged);
                    }
                case 41: //delete-ok
                    {
                        var deleted = protocol.ReadQueueDeleteOk(payload);
                        protocol.Advance();
                        return _channelHandler.OnQueueDeleteOkAsync(header.Channel, deleted);
                    }
                default:
                    throw new Exception($"{nameof(RabbitMQListener)}.{nameof(ProcessQueue)} :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");
            }
        }
        internal ValueTask ProcessBasic(RabbitMQProtocolReader protocol, ref FrameHeader header, ref MethodHeader method, in ReadOnlySequence<byte> payload)
        {
            switch (method.MethodId)
            {
                case 60://deliver method
                    {
                        var deliver = protocol.ReadBasicDeliver(payload);
                        protocol.Advance();
                        return _channelHandler.OnDeliverAsync(header.Channel, deliver);
                    }
                case 21:// consume-ok 
                    {
                        var tag = protocol.ReadBasicConsumeOk(payload);
                        protocol.Advance();
                        return _channelHandler.OnConsumeOkAsync(header.Channel, tag);
                    }
                case 11: // qos-ok
                    {
                        protocol.Advance();
                        return _channelHandler.OnQosOkAsync(header.Channel);
                    }
                case 31://consumer cancel-ok
                    {
                        var tag = protocol.ReadBasicConsumeCancelOk(payload);
                        protocol.Advance();
                        return _channelHandler.OnConsumerCancelOkAsync(header.Channel, tag);
                    }
                default: throw new Exception($"{nameof(RabbitMQListener)}.{nameof(ProcessBasic)}: cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");
            }
        }
    }
}
