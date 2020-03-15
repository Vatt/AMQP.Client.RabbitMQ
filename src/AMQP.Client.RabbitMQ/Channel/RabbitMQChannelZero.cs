using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using Bedrock.Framework;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    /*
     * Zero channel is a service channel 
     */
    internal class RabbitMQChannelZero : IRabbitMQClosable, IRabbitMQOpenable
    {
        private static readonly Bedrock.Framework.Client _client = new ClientBuilder(new ServiceCollection().BuildServiceProvider())
                                                                                     .UseSockets()
                                                                                     .Build();

        public RabbitMQServerInfo ServerInfo { get; private set; }
        public RabbitMQClientInfo ClientInfo { get; private set; }
        public RabbitMQMainInfo MainInfo { get; private set; }
        private CancellationToken _token;
        public EndPoint Endpoint;
        private bool _isClosed;
        private TaskCompletionSource<bool> _openOkSrc;
        private TaskCompletionSource<bool> _closeSrc;
        private TaskCompletionSource<CloseInfo> _connectionClosedSrc;
        private const ushort _channelId = 0;
        public ConnectionContext ConnectionContext;
        private RabbitMQProtocol _protocol;
        public bool IsClosed => _isClosed;

        private readonly RabbitMQConnectionInfo _connectionInfo;
        private Timer _heartbeat;
        private readonly ByteWriter _byteWriter;

        internal RabbitMQChannelZero(RabbitMQConnectionFactoryBuilder builder, TaskCompletionSource<CloseInfo> connectionClosedSrc, CancellationToken token)//:base(protocol)
        {
            MainInfo = builder.MainInfo;
            ClientInfo = builder.ClientInfo;
            _connectionInfo = builder.ConnInfo;
            Endpoint = builder.Endpoint;
            _isClosed = false;
            _connectionClosedSrc = connectionClosedSrc;
            _openOkSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _closeSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _token = token;
            _byteWriter = new ByteWriter();
        }
        public async ValueTask HandleAsync(FrameHeader header)
        {
            Debug.Assert(_channelId == header.Channel);
            Debug.Assert(header.FrameType == Constants.FrameMethod);

            var method = await _protocol.ReadMethodHeader().ConfigureAwait(false);
            await HandleMethod(method).ConfigureAwait(false);
        }

        private async ValueTask HandleMethod(MethodHeader method)
        {
            Debug.Assert(method.ClassId == 10);
            switch (method.MethodId)
            {
                case 10:
                    {
                        ServerInfo = await _protocol.ReadStartAsync().ConfigureAwait(false);
                        await _protocol.SendStartOk(ClientInfo, _connectionInfo).ConfigureAwait(false);
                        break;
                    }

                case 30:
                    {
                        MainInfo = await ProcessTuneMethodAsync().ConfigureAwait(false);
                        await _protocol.SendTuneOk(MainInfo).ConfigureAwait(false);
                        await _protocol.SendOpen(_connectionInfo.VHost).ConfigureAwait(false);
                        break;
                    }
                case 41:
                    {
                        _isClosed = await _protocol.ReadConnectionOpenOkAsync().ConfigureAwait(false);
                        _openOkSrc.SetResult(_isClosed);
                        break;
                    }
                case 50: //close
                    {
                        //await SendCloseOk();
                        _connectionClosedSrc.SetResult(await _protocol.ReadClose().ConfigureAwait(false));
                        break;
                    }
                case 51://close-ok
                    {
                        var closeOk = await _protocol.ReadCloseOk().ConfigureAwait(false);
                        _closeSrc.SetResult(closeOk);
                        break;
                    }

                default:
                    throw new Exception($"{nameof(RabbitMQChannelZero)}:cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
        private async ValueTask<RabbitMQMainInfo> ProcessTuneMethodAsync()
        {
            var info = await _protocol.ReadTuneMethodAsync().ConfigureAwait(false);
            var mainInfo = MainInfo;
            if ((mainInfo.ChannelMax > info.ChannelMax) || (mainInfo.ChannelMax == 0 && info.ChannelMax != 0))
            {
                mainInfo.ChannelMax = info.ChannelMax;
            }
            if (MainInfo.FrameMax > info.FrameMax)
            {
                mainInfo.FrameMax = info.FrameMax;
            }
            return mainInfo;
        }
        public async Task CreateConnection()
        {
            ConnectionContext = await _client.ConnectAsync(Endpoint, _token).ConfigureAwait(false);
        }
        public async Task OpenAsync(RabbitMQProtocol protocol)
        {
            _protocol = protocol;
            await _protocol.SendProtocol().ConfigureAwait(false);
            _heartbeat = new Timer(Heartbeat, null, 0, MainInfo.Heartbeat);

            await _openOkSrc.Task.ConfigureAwait(false);
        }

        public Task<bool> CloseAsync(string reason)
        {
            return CloseAsync(Constants.ReplySuccess, reason, 10, 50);
        }

        public async Task<bool> CloseAsync(short replyCode, string replyText, short failedClassId, short failedMethodId)
        {
            var info = new CloseInfo(replyCode, replyText, failedClassId, failedMethodId);
            await _protocol.SendConnectionCloseAsync(info).ConfigureAwait(false);
            await _closeSrc.Task.ConfigureAwait(false);
            _connectionClosedSrc.SetResult(new CloseInfo(Constants.Success, replyText, 0, 0));
            return true;
        }

        private void Heartbeat(object state)
        {
            _ = HeartbeatAsync();
        }

        private async ValueTask HeartbeatAsync()
        {
            try
            {
                await _protocol.SendHeartbeat().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                //TODO logger
            }
        }
    }
}
