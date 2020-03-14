using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using System.Threading;
using AMQP.Client.RabbitMQ.Protocol.Methods.Common;
using Bedrock.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Connections;
using System.Net;

namespace AMQP.Client.RabbitMQ.Channel
{
    /*
     * Zero channel is a service channel 
     */
    internal class RabbitMQChannelZero :IRabbitMQClosable, IRabbitMQOpenable
    {
        private static readonly Bedrock.Framework.Client _client = new ClientBuilder(new ServiceCollection().BuildServiceProvider())
                                                                                     .UseSockets()
                                                                                     .Build();
        private static readonly byte[] _protocolMsg = new byte[8] { 65, 77, 81, 80, 0, 0, 9, 1 };
        private static byte[] _heartbeatFrame => new byte[8] { 8, 0, 0, 0, 0, 0, 0, 206 };
        public RabbitMQServerInfo ServerInfo { get; private set; }
        public RabbitMQClientInfo ClientInfo { get; private set; }
        public RabbitMQMainInfo MainInfo { get; private set; }
        private  CancellationToken _token;
        public EndPoint Endpoint;
        private bool _isClosed;
        private TaskCompletionSource<bool> _openOkSrc;
        private TaskCompletionSource<bool> _closeSrc;
        private TaskCompletionSource<CloseInfo> _connectionClosedSrc;
        private const ushort _channelId = 0;
        public ConnectionContext ConnectionContext;
        ConnectionReaderWriter _readerWriter;
        private RabbitMQProtocol _protocol;
        public bool IsClosed => _isClosed;

        private readonly RabbitMQConnectionInfo _connectionInfo;
        private Timer _heartbeat;


        internal RabbitMQChannelZero(RabbitMQConnectionFactoryBuilder builder, TaskCompletionSource<CloseInfo> connectionClosedSrc ,CancellationToken token)//:base(protocol)
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
        }
        public async ValueTask HandleAsync(FrameHeader header)
        {
            Debug.Assert(_channelId == header.Channel);
            Debug.Assert(header.FrameType == Constants.FrameMethod);

            var method = await _readerWriter.ReadMethodHeader().ConfigureAwait(false);
            await HandleMethod(method).ConfigureAwait(false);
        }

        private async ValueTask HandleMethod(MethodHeader method)
        {
            Debug.Assert(method.ClassId == 10);
            switch (method.MethodId)
            {
                case 10:
                    {
                        ServerInfo = await _readerWriter.ReadStartAsync().ConfigureAwait(false);
                        await _readerWriter.SendStartOk(ClientInfo,_connectionInfo).ConfigureAwait(false);
                        break;
                    }

                case 30:
                    {
                        MainInfo = await ProcessTuneMethodAsync().ConfigureAwait(false);
                        await _readerWriter.SendTuneOk(MainInfo).ConfigureAwait(false);
                        await _readerWriter.SendOpen(_connectionInfo.VHost).ConfigureAwait(false);
                        break;
                    }
                case 41:
                    {
                        _isClosed = await _readerWriter.ReadOpenOkAsync().ConfigureAwait(false);
                        _openOkSrc.SetResult(_isClosed);
                        break;
                    }
                case 50: //close
                    {
                        //await SendCloseOk();
                        _connectionClosedSrc.SetResult(await _readerWriter.ReadCloseMethod().ConfigureAwait(false));
                        break;
                    }
                case 51://close-ok
                    {
                        _closeSrc.SetResult(await _readerWriter.ReadCloseOk());                                                
                        break;
                    }

                default:
                    throw new Exception($"{nameof(RabbitMQChannelZero)}:cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
        private async ValueTask<RabbitMQMainInfo> ProcessTuneMethodAsync()
        {
            var info = await _readerWriter.ReadTuneMethodAsync().ConfigureAwait(false);
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
            _readerWriter = new ConnectionReaderWriter(_protocol);
            await _protocol.Writer.WriteAsync(new ByteWriter(), _protocolMsg).ConfigureAwait(false);
            _heartbeat = new Timer(async (obj) =>
            {
                await _protocol.Writer.WriteAsync(new ByteWriter(), _heartbeatFrame);
            }, null, 0, MainInfo.Heartbeat);

            await _openOkSrc.Task.ConfigureAwait(false);            
        }

        public Task<bool> CloseAsync(string reason)
        {

            return CloseAsync(Constants.ReplySuccess, reason, 10, 50);
        }
        public async Task<bool> CloseAsync(short replyCode, string replyText, short failedClassId, short failedMethodId)
        {
            var info = new CloseInfo(replyCode, replyText, failedClassId, failedMethodId);
            await _readerWriter.SendCloseMethodAsync(info).ConfigureAwait(false);
            await _closeSrc.Task.ConfigureAwait(false);
            _connectionClosedSrc.SetResult(new CloseInfo(Constants.Success, replyText, 0, 0));
            return true;
        }
    }
}
