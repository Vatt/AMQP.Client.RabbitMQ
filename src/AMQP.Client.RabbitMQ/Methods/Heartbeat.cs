using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Methods
{
    internal class Heartbeat
    {
        private static readonly ReadOnlyMemory<byte> _heartbeatFrame = new byte[8] { 8, 0, 0, 0, 0, 0, 0, 206 };
        private readonly PipeWriter _writer;
        private readonly CancellationToken _closedToken;
        private int _missedServerHeartbeats;
        private Timer _timer;
        public int MissedServerHeartbeats => _missedServerHeartbeats;
        public Heartbeat(PipeWriter writer,CancellationToken token = default)
        {
            _writer = writer;
            _closedToken = token;
            
            _missedServerHeartbeats = 0;
        }
        public Task StartAsync()
        {
            _timer = new Timer(async (_) => { TickHeartbeat(); }, this, 60, 60);
            return default;
        }
        public void OnHeartbeat(ReadOnlySequence<byte> sequence)
        {
            Interlocked.Exchange(ref _missedServerHeartbeats, 0);
            if (sequence.FirstSpan[7] != 206)
            {

            }
        }
        public async ValueTask TickHeartbeat()
        {
            var memory = _writer.GetMemory(8);
            _heartbeatFrame.CopyTo(memory);
            _writer.Advance(8);
            await _writer.FlushAsync();

        }
    }
}
