using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Internal;

namespace AMQP.Client.RabbitMQ.Channel
{
    /*
     * Zero channel is a service channel 
     */
    internal class RabbitMQChannelZero : ConnectionReaderWriter, IRabbitMQChannel
    {
        private static readonly byte[] _protocolMsg = new byte[8] { 65, 77, 81, 80, 0, 0, 9, 1 };
        public RabbitMQServerInfo ServerInfo { get; private set; }
        public RabbitMQClientInfo ClientInfo { get; private set; }

        public RabbitMQMainInfo MainInfo { get; private set; }

        private bool _isOpen;
        private TaskCompletionSource<bool> _openOkSrc = new TaskCompletionSource<bool>();
        private readonly ushort _channelId;

        public bool IsOpen => _isOpen;
        public ushort ChannelId => _channelId;

        private readonly RabbitMQConnectionInfo _connectionInfo;
        

        internal RabbitMQChannelZero(RabbitMQConnectionBuilder builder, RabbitMQProtocol protocol):base(protocol)
        {
            MainInfo = builder.MainInfo;
            ClientInfo = builder.ClientInfo;
            _connectionInfo = builder.ConnInfo;
            _channelId = 0;
            _isOpen = false;
        }
        public async ValueTask HandleAsync(FrameHeader header)
        {
            Debug.Assert(_channelId == header.Channel);
            switch (header.FrameType)
            {
                case 1:
                    {
                        var method = await ReadMethodHeader().ConfigureAwait(false);
                        await HandleMethod(method).ConfigureAwait(false);
                        break;
                    }
            }
        }

        private async ValueTask HandleMethod(MethodHeader method)
        {
            Debug.Assert(method.ClassId == 10);
            switch (method.ClassId)
            {
                case 10 when method.MethodId == 10:
                    {
                        ServerInfo = await ReadStartAsync().ConfigureAwait(false);
                        await SendStartOk(ClientInfo,_connectionInfo).ConfigureAwait(false);
                        break;
                    }

                case 10 when method.MethodId == 30:
                    {
                        MainInfo = await ProcessTuneMethodAsync().ConfigureAwait(false);
                        await SendTuneOk(MainInfo).ConfigureAwait(false);
                        await SendOpen(_connectionInfo.VHost).ConfigureAwait(false);
                        break;
                    }
                case 10 when method.MethodId == 41:
                    {
                        _isOpen = await ReadOpenOkAsync().ConfigureAwait(false);
                        _openOkSrc.SetResult(_isOpen);
                        break;
                    }
                case 10 when method.MethodId == 50: //close-ok
                    {
                        break;
                    }

                default:
                    throw new Exception($"{nameof(RabbitMQChannelZero)}:cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }

        private async ValueTask<RabbitMQMainInfo> ProcessTuneMethodAsync()
        {
            var info = await ReadTuneMethodAsync().ConfigureAwait(false);
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

        public async Task<bool> TryOpenChannelAsync()
        {
            await _protocol.Writer.WriteAsync(new ByteWriter(), _protocolMsg).ConfigureAwait(false);
            return await _openOkSrc.Task.ConfigureAwait(false);
        }

        public Task<bool> TryCloseChannelAsync(string reason)
        {
            //тиснуть сюда Close, CloseOK методы
            return Task.FromResult(false);
        }
        public Task<bool> TryCloseChannelAsync(short replyCode, string replyText, short failedClassId, short failedMethodId)
        {
            //тиснуть сюда Close, CloseOK методы
            return Task.FromResult(false);
        }
    }
}
