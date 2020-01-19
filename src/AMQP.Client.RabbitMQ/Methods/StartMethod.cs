using AMQP.Client.RabbitMQ.Decoder;
using AMQP.Client.RabbitMQ.Encoder;
using AMQP.Client.RabbitMQ.Internal;
using Microsoft.AspNetCore.Connections;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Methods
{
    internal class StartMethod
    {
        private static readonly byte[] _protocol = new byte[8] { 65, 77, 81, 80, 0, 0, 9, 1 };
        private readonly ReaderDispatcher _dispatcher;
        private readonly PipeWriter _writer; //FOR TEST
        internal RabbitMQServerInfo _serverInfo;
        public StartMethod(ReaderDispatcher dispatcher,PipeWriter writer)
        {
            _dispatcher = dispatcher;
            _writer = writer;
        }
        public async Task InvokeAsync()
        {
            _dispatcher.SetWait(DecodeStart);
            await SendProtocol();
            await SendStartOk();
            
            
        }
        private void EncodeStartOk(Memory<byte> memory)
        {     
            FrameEncoder.EncodeStartOkFrame(memory);
        }
        private void DecodeStart(ReadOnlySequence<byte> sequence)
        {

            _serverInfo = FrameDecoder.DecodeStartMethodFrame(sequence);
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
