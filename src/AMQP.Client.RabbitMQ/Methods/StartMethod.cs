using AMQP.Client.RabbitMQ.Decoder;
using AMQP.Client.RabbitMQ.Encoder;
using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Methods
{
    internal class StartMethod
    {
        private static readonly byte[] _protocol = new byte[8] { 65, 77, 81, 80, 0, 0, 9, 1 };
        private readonly RabbitMQListener _reader;
        private readonly PipeWriter _writer; 
        private Action<RabbitMQServerInfo> _serverInfoCalback;
        private readonly RabbitMQInfo _info;
        private readonly RabbitMQConnectionInfo _connectionInfo;
        private readonly RabbitMQClientInfo _clientInfo;
        public readonly  Action _successCallback;
        public StartMethod(RabbitMQListener reader, PipeWriter writer, RabbitMQInfo info,
                           RabbitMQConnectionInfo connectionInfo,RabbitMQClientInfo clientInfo,
                           Action<RabbitMQServerInfo> serverInfoCalback, Action successCallback)
        {
            _reader = reader;
            _writer = writer;
            _serverInfoCalback = serverInfoCalback;
            _info = info;
            _connectionInfo = connectionInfo;
            _clientInfo = clientInfo;
            _successCallback = successCallback;
            _reader.SubscribeOnMethod(new MethodFrame(10,10), ProcessStart);
            _reader.SubscribeOnMethod(new MethodFrame(10,30), ProcessTuneAndOpen);
            _reader.SubscribeOnMethod(new MethodFrame(10,41), OpenOk);
        }

        private ValueTask OpenOk(ReadOnlySequence<byte> sequence)
        {
            _reader.AdvanceTo(sequence.End);
            _successCallback();
            return default;
        }

        public async ValueTask RunAsync()
        {
            await SendProtocol();          
        }

        private async ValueTask ProcessStart(ReadOnlySequence<byte> sequence)
        {
            var position = FrameDecoder.DecodeStartMethodFrame(sequence, out RabbitMQServerInfo info);
            _reader.AdvanceTo(position);
            _serverInfoCalback(info);
            await SendStartOk();
        }

        private async ValueTask SendStartOk()
        {
            var memory = _writer.GetMemory();                       
            _writer.Advance(FrameEncoder.EncodeStartOkFrame(memory, _clientInfo, _connectionInfo));
            await _writer.FlushAsync();
        }
        private async Task SendProtocol()
        {
            Memory<byte> memory = _writer.GetMemory(8);
            _protocol.CopyTo(memory);
            _writer.Advance(8);
            await _writer.FlushAsync();            
        }
        private async ValueTask ProcessTuneAndOpen(ReadOnlySequence<byte> sequence)
        {
            var position = FrameDecoder.DecodeTuneMethodFrame(sequence, out RabbitMQInfo connectionInfo); // надо чтото сдеать с этим
            _reader.AdvanceTo(position);
            await SendTuneOkFrame();
            await ProcessOpenSequence();
        }
        private async ValueTask ProcessOpenSequence()
        {
            Memory<byte> memory = _writer.GetMemory();
            _writer.Advance(FrameEncoder.EncodeOpenFrame(memory,_connectionInfo.VHost));
            await _writer.FlushAsync();
        }
        private async ValueTask SendTuneOkFrame()
        {
            Memory<byte> memory = _writer.GetMemory(20);
            _writer.Advance(FrameEncoder.EncodeTuneOKFrame(memory,_info));
            await _writer.FlushAsync();

        }

    }
}
