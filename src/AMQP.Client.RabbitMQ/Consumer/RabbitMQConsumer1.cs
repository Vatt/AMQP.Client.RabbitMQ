namespace AMQP.Client.RabbitMQ.Consumer
{
    /*
        public class RabbitMQConsumer1 : ConsumerBase
        {
            private IChunkedBodyFrameReader _reader;
            public event EventHandler<DeliverArgs> Received;
            private readonly PipeScheduler _scheduler;
            private byte[] _activeDeliver;
            private int _deliverPosition;


            public RabbitMQConsumer1(ConsumeConf info, PipeScheduler scheduler, RabbitMQChannel channel)
                : base(info, channel)
            {
                _reader = new BodyFrameChunkedReader(Channel.ChannelId);
                _scheduler = scheduler;
                _deliverPosition = 0;
            }

            internal override async ValueTask ProcessBodyMessage(RabbitMQProtocolReader protocol, RabbitMQDeliver deliver)
            {
                _deliverPosition = 0;
                _activeDeliver = ArrayPool<byte>.Shared.Rent((int)deliver.Header.BodySize);
                _reader.Reset(deliver.Header.BodySize);

                while (!_reader.IsComplete)
                {
                    var result = await protocol.ReadWithoutAdvanceAsync(_reader).ConfigureAwait(false);
                    Copy(result);
                    protocol.Advance();
                }

                var arg = new DeliverArgs(ref deliver, _activeDeliver);
                _scheduler.Schedule(Invoke, arg);
            }

            private void Invoke(object obj)
            {
                if (obj is DeliverArgs arg)
                {
                    try
                    {
                        Received?.Invoke(this, arg);
                    }
                    catch (Exception e)
                    {
                        // add logger
                        Debugger.Break();

                    }
                    finally
                    {
                        arg.Dispose();
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Copy(ReadOnlySequence<byte> message)
            {
                var span = new Span<byte>(_activeDeliver, _deliverPosition, (int)message.Length);
                message.CopyTo(span);
                _deliverPosition += (int)message.Length;
            }
        }

        public class DeliverArgs1 : EventArgs, IDisposable
        {
            public ContentHeaderProperties Properties { get; }
            public long DeliveryTag { get; }
            private byte[] _body;
            private int _bodySize;

            public ReadOnlySpan<byte> Body => new ReadOnlySpan<byte>(_body, 0, _bodySize);

            internal DeliverArgs1(ref RabbitMQDeliver deliverInfo, byte[] body)
            {
                Properties = deliverInfo.Header.Properties;
                DeliveryTag = deliverInfo.DeliveryTag;
                _body = body;
                _bodySize = (int)deliverInfo.Header.BodySize;
            }

            public void Dispose()
            {
                if (_body != null)
                {
                    ArrayPool<byte>.Shared.Return(_body);
                    _body = null;
                }
            }
        }
        */

}
