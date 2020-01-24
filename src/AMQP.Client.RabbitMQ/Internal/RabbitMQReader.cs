using AMQP.Client.RabbitMQ.Decoder;
using AMQP.Client.RabbitMQ.Framing;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Internal
{

    public class RabbitMQReader
    {
        private readonly PipeReader _reader;
        private readonly Dictionary<MethodFrame,Action<ReadOnlySequence<byte>>> _methodsCallbacks;
        public RabbitMQReader(PipeReader reader)
        {
            _reader = reader;
            _methodsCallbacks = new Dictionary<MethodFrame, Action<ReadOnlySequence<byte>>>();
        }
        public async Task StartAsync()
        {
            while (true)
            {
                var result = await _reader.ReadAsync();
                var frame = FrameDecoder.DecodeFrame(result.Buffer);
                switch (frame.FrameType)
                {
                    case 1:OnMethod(result.Buffer);break;
                }                
            }
        }
        public void OnMethod(ReadOnlySequence<byte> sequence)
        {
            var methodFrame = FrameDecoder.DecodeMethodFrame(sequence);
            var result = _methodsCallbacks.TryGetValue(methodFrame, out Action<ReadOnlySequence<byte>> callback);
            if(!result)
            {
                throw new Exception($"RabbitMQReader.OnMethod with (class-id,method-id)={(methodFrame.ClassId, methodFrame.MethodId)}");
            }
            callback(sequence);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceTo(SequencePosition position)
        {
            _reader.AdvanceTo(position);
        }
        public void Subscribe(MethodFrame frame, Action<ReadOnlySequence<byte>> callback)
        {
            if(_methodsCallbacks.ContainsKey(frame))
            {
                throw new Exception($"RabbitMQReader.Subscribe (class-id,method-id)={(frame.ClassId, frame.MethodId)}");
            }
            _methodsCallbacks.Add(frame, callback);
        }
        
    }
}
