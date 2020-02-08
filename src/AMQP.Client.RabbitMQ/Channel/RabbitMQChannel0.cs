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
    public class RabbitMQChannel0 : RabbitMQChannelBase
    {
        private static readonly byte[] _protocolMsg = new byte[8] { 65, 77, 81, 80, 0, 0, 9, 1 };
        public RabbitMQServerInfo ServerInfo { get; private set; }
        public RabbitMQClientInfo ClientInfo { get; private set; }
        private RabbitMQMainInfo _mainInfo;
        public RabbitMQMainInfo MainInfo => _mainInfo;

        public override bool IsOpen => false;

        private readonly RabbitMQConnectionInfo _connectionInfo;
        private readonly RabbitMQProtocol _protocol;
        public RabbitMQChannel0(RabbitMQConnectionBuilder builder, RabbitMQProtocol protocol) :base(0)
        {
            _mainInfo = builder.MainInfo;
            ClientInfo = builder.ClientInfo;
            _connectionInfo = builder.ConnInfo;
            _protocol = protocol;
        }
        public override async ValueTask HandleFrameAsync(FrameHeader header)
        {
            Debug.Assert(this.Id == header.Chanell);
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
                        await ReadStartMethodAsync();
                        await SendStartOk();
                        break;
                    }
                
                case 10 when method.MethodId == 30:
                    {
                        await ReadTuneMethodAsync();
                        break;
                    }
                
                default:
                    throw new Exception($"{nameof(RabbitMQChannel0)}:cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId}");

            }
        }
        private async ValueTask SendStartOk()
        {
            var startok = new ConnectionStartOkWriter(_connectionInfo);
            await _protocol.Writer.WriteAsync(startok, ClientInfo);
        }
        private async ValueTask ReadStartMethodAsync()
        {
            var result = await _protocol.Reader.ReadAsync(new ConnectionStartReader());
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            ServerInfo = result.Message;
            _protocol.Reader.Advance();
        }
        private async ValueTask ReadTuneMethodAsync()
        {
            var result =  await _protocol.Reader.ReadAsync(new ConnectionTuneReader());
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            var info = result.Message;
            if ((_mainInfo.ChanellMax > info.ChanellMax) || (_mainInfo.ChanellMax == 0 && info.ChanellMax != 0))
            {
                _mainInfo.ChanellMax = info.ChanellMax;
            }
            if (MainInfo.FrameMax > info.FrameMax)
            {
                _mainInfo.FrameMax = info.FrameMax;
            }
        }
        public override async ValueTask<bool> TryOpenChannelAsync()
        {
            await _protocol.Writer.WriteAsync(new ByteWriter(), _protocolMsg);
            return true;
        }

        public override ValueTask<bool> TryCloseChannelAsync()
        {
            //тиснуть сюда Close, CloseOK методы
            return default; ;
        }
    }
}
