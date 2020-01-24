using AMQP.Client.RabbitMQ.Decoder;
using AMQP.Client.RabbitMQ.Encoder;
using AMQP.Client.RabbitMQ.Framing;
using AMQP.Client.RabbitMQ.Internal;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Methods
{
    internal class StartMethod
    {
        private static readonly byte[] _protocol = new byte[8] { 65, 77, 81, 80, 0, 0, 9, 1 };
        private readonly RabbitMQReader _reader;
        private readonly PipeWriter _writer; //FOR TEST
        private Action<RabbitMQServerInfo> _callback;
        public StartMethod(RabbitMQReader reader, PipeWriter writer, Action<RabbitMQServerInfo> callback)
        {
            _reader = reader;
            _writer = writer;
            _callback = callback;
            _reader.Subscribe(new MethodFrame(10,10), PorocessStartSequence);
        }
        public async void RunAsync()
        {
            await SendProtocol();          
        }
        private void EncodeStartOk(Memory<byte> memory)
        {     
            FrameEncoder.EncodeStartOkFrame(memory);
        }
        private async void PorocessStartSequence(ReadOnlySequence<byte> sequence)
        {
            DecodeStart(sequence);
            await SendStartOk();
        }
        private void DecodeStart(ReadOnlySequence<byte> sequence)
        {
            var position =  FrameDecoder.DecodeStartMethodFrame(sequence, out RabbitMQServerInfo info);
            _reader.AdvanceTo(position);
            _callback(info);
        }
        private async Task SendStartOk()
        {
            var memory = _writer.GetMemory();
            EncodeStartOk(memory);
            _writer.Advance(8);
            await _writer.FlushAsync();
        }
        private async Task SendProtocol()
        {
            Memory<byte> memory = _writer.GetMemory(8);
            _protocol.CopyTo(memory);
            _writer.Advance(8);
            var flush = await _writer.FlushAsync();            
        }

    }
}
