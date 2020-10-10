using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Exceptions;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Channel;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;

namespace AMQP.Client.RabbitMQ.Protocol
{

    public class RabbitMQListener
    {
        private ProtocolReader _protocol;
        private IChannelHandler _channelHandler;
        private IConnectionHandler _connectionHandler;
        private bool _isClosed;

        public bool IsClosed => _isClosed;
        public async Task StartAsync(ProtocolReader reader, IConnectionHandler connection, IChannelHandler channel, CancellationToken token = default)
        {
            _connectionHandler = connection;
            _channelHandler = channel;
            _isClosed = false;
            _protocol = reader;
            //while (true)
            while (!_isClosed)
            {
                var result = await ReadAsync(ProtocolReaders.FrameHeaderReader, token).ConfigureAwait(false);
                switch (result.FrameType)
                {
                    case RabbitMQConstants.FrameMethod:
                        {
                            var method = await ReadAsync(ProtocolReaders.MethodHeaderReader, token).ConfigureAwait(false);
                            await ProcessMethod(reader, ref result, ref method, token).ConfigureAwait(false);
                            break;
                        }
                    case RabbitMQConstants.FrameHeartbeat:
                        {
                            await ReadAsync(ProtocolReaders.NoPayloadReader, token).ConfigureAwait(false);
                            await _connectionHandler.OnHeartbeatAsync().ConfigureAwait(false);
                            break;
                        }
                    default:
                        {
                            throw new RabbitMQFrameException(result.FrameType);
                        }
                }
            }
        }

        private async ValueTask<T> ReadAsync<T>(IMessageReader<T> reader, CancellationToken token = default)
        {
            var result = await _protocol.ReadAsync(reader, token).ConfigureAwait(false);
            _protocol.Advance();
            if (result.IsCanceled || result.IsCompleted)
            {
                //TODO: do something
            }

            return result.Message;
        }
        public void Stop()
        {
            _isClosed = true;
        }
        internal ValueTask ProcessMethod(ProtocolReader protocol, ref FrameHeader header, ref MethodHeader method, CancellationToken token = default)
        {

            switch (method.ClassId)
            {
                case 10:
                    {
                        return ProcessConnection(protocol, header, method, token);
                    }
                case 20:
                    {
                        return ProcessChannel(protocol, header, method, token);
                    }
                case 40:
                    {
                        return ProcessExchange(protocol, header, method, token);
                    }
                case 50:
                    {
                        return ProcessQueue(protocol, header, method, token);
                    }

                case 60:
                    {
                        return ProcessBasic(protocol, header, method, token);
                    }

                default:
                    {
                        throw new RabbitMQMethodException(nameof(ProcessMethod), method.ClassId, method.MethodId);
                    }
            }
        }

        internal async ValueTask ProcessConnection(ProtocolReader protocol, FrameHeader header, MethodHeader method, CancellationToken token = default)
        {
            if (header.Channel != 0) ReaderThrowHelper.ThrowIfFrameTypeMissmatch();
            switch (method.MethodId)
            {
                case 10:
                    {
                        var serverConf = await ReadAsync(ProtocolReaders.ConnectionStartReader, token).ConfigureAwait(false);
                        await _connectionHandler.OnStartAsync(serverConf).ConfigureAwait(false);
                        break;
                    }

                case 30:
                    {
                        var tuneConf = await ReadAsync(ProtocolReaders.ConnectionTuneReader, token).ConfigureAwait(false);
                        await _connectionHandler.OnTuneAsync(tuneConf).ConfigureAwait(false);
                        break;
                    }
                case 41:
                    {
                        var result = await ReadAsync(ProtocolReaders.ConnectionOpenOkReader, token).ConfigureAwait(false);
                        await _connectionHandler.OnOpenOkAsync();
                        break;
                    }
                case 50: //close
                    {
                        var closeInfo = await ReadAsync(ProtocolReaders.CloseReader, token).ConfigureAwait(false);
                        await _connectionHandler.OnCloseAsync(closeInfo).ConfigureAwait(false);
                        break;
                    }
                case 51: //close-ok
                    {
                        var closeOk = await ReadAsync(ProtocolReaders.NoPayloadReader, token).ConfigureAwait(false);
                        await _connectionHandler.OnCloseOkAsync().ConfigureAwait(false);
                        break;
                    }
                default:
                    throw new RabbitMQMethodException(nameof(ProcessConnection), method.ClassId, method.MethodId);
            }
        }

        internal async ValueTask ProcessChannel(ProtocolReader protocol, FrameHeader header, MethodHeader method, CancellationToken token = default)
        {
            switch (method.MethodId)
            {
                case 11: //open-ok
                    {
                        var openOk = await ReadAsync(ProtocolReaders.ChannelOpenOkReader, token).ConfigureAwait(false);
                        await _channelHandler.OnChannelOpenOkAsync(header.Channel).ConfigureAwait(false);
                        break;
                    }
                case 40: //close
                    {
                        var closeInfo = await ReadAsync(ProtocolReaders.CloseReader, token).ConfigureAwait(false); 
                        await _channelHandler.OnChannelCloseAsync(header.Channel, closeInfo).ConfigureAwait(false);
                        break;
                    }
                case 41: //close-ok
                    {
                        var closeOk = await ReadAsync(ProtocolReaders.NoPayloadReader, token).ConfigureAwait(false);
                        await _channelHandler.OnChannelCloseOkAsync(header.Channel).ConfigureAwait(false);
                        break;
                    }
                default:
                    throw new RabbitMQMethodException(nameof(ProcessChannel), method.ClassId, method.MethodId);
            }
        }

        internal async ValueTask ProcessExchange(ProtocolReader protocol, FrameHeader header, MethodHeader method, CancellationToken token = default)
        {
            switch (method.MethodId)
            {
                case 11: //declare-ok
                    {
                        var declareOk = await ReadAsync(ProtocolReaders.NoPayloadReader, token).ConfigureAwait(false); 
                        await _channelHandler.OnExchangeDeclareOkAsync(header.Channel).ConfigureAwait(false); 
                        break;
                    }
                case 21: //delete-ok
                    {
                        var declareOk = await ReadAsync(ProtocolReaders.NoPayloadReader, token).ConfigureAwait(false); 
                        await _channelHandler.OnExchangeDeleteOkAsync(header.Channel).ConfigureAwait(false);
                        break;
                    }
                default:
                    throw new RabbitMQMethodException(nameof(ProcessExchange), method.ClassId, method.MethodId);
            }
        }

        internal async ValueTask ProcessQueue(ProtocolReader protocol, FrameHeader header, MethodHeader method, CancellationToken token = default)
        {
            switch (method.MethodId)
            {
                case 11: //declare-ok
                    {
                        var declareOk = await ReadAsync(ProtocolReaders.QueueDeclareOkReader, token).ConfigureAwait(false);
                        await _channelHandler.OnQueueDeclareOkAsync(header.Channel, declareOk).ConfigureAwait(false);
                        break;
                    }
                case 21: //bind-ok
                    {
                        var bindOk = await ReadAsync(ProtocolReaders.NoPayloadReader, token).ConfigureAwait(false); 
                        await _channelHandler.OnQueueBindOkAsync(header.Channel).ConfigureAwait(false);
                        break;
                    }
                case 51: //unbind-ok
                    {
                        var unbindOk = await ReadAsync(ProtocolReaders.NoPayloadReader, token).ConfigureAwait(false); 
                        await _channelHandler.OnQueueUnbindOkAsync(header.Channel).ConfigureAwait(false);
                        break;
                    }
                case 31: //purge-ok
                    {
                        var purged = await ReadAsync(ProtocolReaders.QueuePurgeOkDeleteOkReader, token).ConfigureAwait(false);
                        await _channelHandler.OnQueuePurgeOkAsync(header.Channel, purged).ConfigureAwait(false);
                        break;
                    }
                case 41: //delete-ok
                    {
                        var deleted = await ReadAsync(ProtocolReaders.QueuePurgeOkDeleteOkReader, token).ConfigureAwait(false);
                        await _channelHandler.OnQueueDeleteOkAsync(header.Channel, deleted).ConfigureAwait(false);
                        break;
                    }
                default:
                    throw new RabbitMQMethodException(nameof(ProcessQueue), method.ClassId, method.MethodId);
            }
        }

        internal async ValueTask ProcessBasic(ProtocolReader protocol, FrameHeader header, MethodHeader method, CancellationToken token = default)
        {
            switch (method.MethodId)
            {
                case 60: //deliver method
                    {
                        var deliver = await ReadAsync(ProtocolReaders.BasicDeliverReader, token).ConfigureAwait(false);
                        await _channelHandler.OnBeginDeliveryAsync(header.Channel, deliver, protocol).ConfigureAwait(false);
                        break;
                    }
                case 21: // consume-ok 
                    {
                        var tag = await ReadAsync(ProtocolReaders.ShortStrPayloadReader, token).ConfigureAwait(false);
                        await _channelHandler.OnConsumeOkAsync(header.Channel, tag).ConfigureAwait(false);
                        break;
                    }
                case 11: // qos-ok
                    {
                        var qosOk = await ReadAsync(ProtocolReaders.NoPayloadReader, token).ConfigureAwait(false);
                        await _channelHandler.OnQosOkAsync(header.Channel).ConfigureAwait(false);
                        break;
                    }
                case 31: //consumer cancel-ok
                    {
                        var tag = await ReadAsync(ProtocolReaders.ShortStrPayloadReader, token).ConfigureAwait(false);
                        await _channelHandler.OnConsumeCancelOkAsync(header.Channel, tag).ConfigureAwait(false);
                        break;
                    }
                case 30:
                    {
                        var cancelInfo = await ReadAsync(ProtocolReaders.BasicConsumeCancelReader, token).ConfigureAwait(false);
                        await _channelHandler.OnConsumeCancelAsync(header.Channel, cancelInfo);
                        break;
                    }
                default:
                    throw new RabbitMQMethodException(nameof(ProcessBasic), method.ClassId, method.MethodId);
            }
        }
    }
}