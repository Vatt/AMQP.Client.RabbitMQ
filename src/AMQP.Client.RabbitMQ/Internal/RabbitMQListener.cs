using AMQP.Client.RabbitMQ.Decoder;
using AMQP.Client.RabbitMQ.Framing;
using AMQP.Client.RabbitMQ.Methods;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Internal
{
    internal delegate ValueTask MethodFrameDelegate(ReadOnlySequence<byte> sequence);
    internal class RabbitMQListener
    {
        private readonly PipeReader _reader;
        private readonly Dictionary<MethodFrame, MethodFrameDelegate> _methodsCallbacks;
        private readonly Heartbeat _heartbeat;
        public RabbitMQListener(PipeReader reader, Heartbeat heartbeat)
        {
            _reader = reader;
            _methodsCallbacks = new Dictionary<MethodFrame, MethodFrameDelegate>();
            _heartbeat = heartbeat;
        }
        public async Task StartAsync()
        {
            while (true)
            {
                var result = await _reader.ReadAsync();
                var frame = FrameDecoder.DecodeFrame(result.Buffer);
                switch (frame.FrameType)
                {
                    case 1:
                        {
                            await OnMethod(result.Buffer);
                            break;
                        }
                    case 8:
                        {
                            _heartbeat.OnHeartbeat(result.Buffer);
                            _reader.AdvanceTo(result.Buffer.GetPosition(8));
                            break;
                        }
                    default: throw new Exception($"RabbitMQListener:cannot decode frame (type,chanell,payload) - {frame.FrameType} {frame.Chanell} {frame.PaylodaSize}." +
                                                 $"Frame data: {Encoding.UTF8.GetString(result.Buffer.ToArray())}");
                }                
            }
        }
        public async ValueTask OnMethod(ReadOnlySequence<byte> sequence)
        {
            var methodFrame = FrameDecoder.DecodeMethodFrame(sequence);
            var result = _methodsCallbacks.TryGetValue(methodFrame, out MethodFrameDelegate callback);
            if(!result)
            {
                throw new Exception($"RabbitMQListener.OnMethod with (class-id,method-id)={(methodFrame.ClassId, methodFrame.MethodId)}");
            }
            await callback(sequence);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceTo(SequencePosition position)
        {
            _reader.AdvanceTo(position);
        }
        public void SubscribeOnMethod(MethodFrame frame, MethodFrameDelegate callback)
        {
            if(_methodsCallbacks.ContainsKey(frame))
            {
                throw new Exception($"RabbitMQListener.Subscribe (class-id,method-id)={(frame.ClassId, frame.MethodId)}");
            }
            _methodsCallbacks.Add(frame, callback);
        }
        
    }
}
