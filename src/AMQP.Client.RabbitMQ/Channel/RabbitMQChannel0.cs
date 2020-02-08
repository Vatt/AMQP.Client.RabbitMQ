using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Info;
using AMQP.Client.RabbitMQ.Protocol.MethodReaders;
using AMQP.Client.RabbitMQ.Protocol.MethodWriters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    /*
     * Zero channel is service channel 
     */
    public class RabbitMQChannel0 : IRabbitMQChannel
    {
        private static readonly byte[] _protocolMsg = new byte[8] { 65, 77, 81, 80, 0, 0, 9, 1 };
        public RabbitMQServerInfo ServerInfo { get; private set; }
        public RabbitMQClientInfo ClientInfo { get; private set; }

        public RabbitMQMainInfo MainInfo { get; private set; }

        private bool _isOpen;
        
        private readonly short _channelId;

        public bool IsOpen => _isOpen;
        public short ChannelId => _channelId;

        private readonly RabbitMQConnectionInfo _connectionInfo;
        private readonly RabbitMQProtocol _protocol;
        

        public RabbitMQChannel0(RabbitMQConnectionBuilder builder, RabbitMQProtocol protocol)
        {
            MainInfo = builder.MainInfo;
            ClientInfo = builder.ClientInfo;
            _connectionInfo = builder.ConnInfo;
            _protocol = protocol;
            _channelId = 0;
            _isOpen = false;
        }
        public async ValueTask HandleAsync(FrameHeader header)
        {
            Debug.Assert(_channelId == header.Chanell);
            switch (header.FrameType)
            {
                case 1:
                    {
                        var method = await ReadMethod();
                        await ProcessMethod(method);
                        break;
                    }
            }
        }
        private async ValueTask<MethodHeader> ReadMethod()
        {
            var result = await _protocol.Reader.ReadAsync(new MethodHeaderReader());
            _protocol.Reader.Advance();
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            return result.Message;
        }
        private async ValueTask ProcessMethod(MethodHeader method)
        {
            Debug.Assert(method.ClassId == 10);
            switch (method.ClassId)
            {
                case 10 when method.MethodId == 10:
                    {
                        ServerInfo = await ReadStartsync();
                        await SendStartOk();
                        break;
                    }

                case 10 when method.MethodId == 30:
                    {
                        MainInfo = await ReadTuneMethodAsync();
                        await SendTuneOk();
                        await SendOpen();
                        break;
                    }
                case 10 when method.MethodId == 41:
                    {
                        _isOpen = await ReadOpenOkAsync();
                        break;
                    }

                default:
                    throw new Exception($"{nameof(RabbitMQChannel0)}:cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
        private async ValueTask SendStartOk()
        {
            var startok = new ConnectionStartOkWriter(_connectionInfo);
            await _protocol.Writer.WriteAsync(startok, ClientInfo);
        }
        private async ValueTask SendTuneOk()
        {
            var tuneok = new ConnectionTuneOkWriter();
            await _protocol.Writer.WriteAsync(tuneok, MainInfo);
        }
        private async ValueTask SendOpen()
        {
            var open = new ConnectionOpenWriter();
            await _protocol.Writer.WriteAsync(open, _connectionInfo.VHost);
        }
        private async ValueTask<RabbitMQServerInfo> ReadStartsync()
        {
            var result = await _protocol.Reader.ReadAsync(new ConnectionStartReader());
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
        private async ValueTask<RabbitMQMainInfo> ReadTuneMethodAsync()
        {
            var result = await _protocol.Reader.ReadAsync(new ConnectionTuneReader());
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            var info = result.Message;
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
        private async ValueTask<bool> ReadOpenOkAsync()
        {
            var result = await _protocol.Reader.ReadAsync(new ConnectionOpenOkReader());
            if (result.IsCompleted)
            {
                //TODO: сделать чтонибудь
            }
            var isOpen = result.Message;
            _protocol.Reader.Advance();
            return isOpen;

        }
        public async ValueTask<bool> TryOpenChannelAsync()
        {
            await _protocol.Writer.WriteAsync(new ByteWriter(), _protocolMsg);
            return true;
        }

        public async ValueTask<bool> TryCloseChannelAsync()
        {
            //тиснуть сюда Close, CloseOK методы
            return default;
        }
    }
}
