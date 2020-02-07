using AMQP.Client.RabbitMQ.Protocol;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Methods
{
    internal class StartMethod
    {
        private static readonly byte[] _protocol = new byte[8] { 65, 77, 81, 80, 0, 0, 9, 1 };
        private readonly RabbitMQWriter _writer; 
        public StartMethod(RabbitMQWriter writer)
        {
            _writer = writer;
            /*
            _reader.SubscribeOnMethod(new MethodHeader(10,10), ProcessStart);
            _reader.SubscribeOnMethod(new MethodHeader(10,30), ProcessTuneAndOpen);
            _reader.SubscribeOnMethod(new MethodHeader(10,41), OpenOk);
            */
        }
        public async ValueTask RunAsync()
        {
            await SendProtocol();
        }
        private async ValueTask SendProtocol()
        {
            _writer.Write(_protocol);
            await _writer.FlushAsync();
        }
        /*
        private ValueTask OpenOk(ReadOnlySequence<byte> sequence)
        {
            _reader.AdvanceTo(sequence.End);
            _successCallback();
            return default;
        }
        

        
        private async ValueTask ProcessStart(ReadOnlySequence<byte> sequence)
        {
            await SendStartOk();
        }
        
        private async ValueTask SendStartOk()
        {                            
            //_writer.WriteStartOk()
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
        */

    }
}
