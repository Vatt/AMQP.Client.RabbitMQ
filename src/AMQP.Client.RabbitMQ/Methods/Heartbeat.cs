using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Methods
{
    internal class Heartbeat:IDisposable
    {
        private static readonly ReadOnlyMemory<byte> _heartbeatFrame = new byte[8] { 8, 0, 0, 0, 0, 0, 0, 206 };
        private readonly PipeWriter _writer;
        private readonly CancellationToken _closedToken;
        private int _missedServerHeartbeats;
        private Timer _timer;
        private Timer _watcher;
        private TimeSpan _tick;
        public int MissedServerHeartbeats => _missedServerHeartbeats;
        public Heartbeat(PipeWriter writer, TimeSpan tick, CancellationToken token = default)
        {
            _writer = writer;
            _closedToken = token;
            _tick = tick;
            _missedServerHeartbeats = 0;
        }
        public Task StartAsync()
        {
            _timer = new Timer(async (_) => { await TickHeartbeat(); }, this, 0, _tick.Ticks);
            return default;
        }
        public void OnHeartbeat(ReadOnlySequence<byte> sequence)
        {
            //Interlocked.Exchange(ref _missedServerHeartbeats, 0);
            SequenceReader<byte> reader = new SequenceReader<byte>(sequence);            
            reader.Advance(7);
            reader.TryRead(out byte val);
            Debug.Assert(val == 206);
        }
        public async ValueTask TickHeartbeat()
        {
            var memory = _writer.GetMemory(8);
            _heartbeatFrame.CopyTo(memory);
            _writer.Advance(8);
            await _writer.FlushAsync();

        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
