﻿using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public class DeliverArgs : EventArgs, IDisposable
    {
        public ContentHeaderProperties Properties { get; }
        public long DeliveryTag { get; }
        private byte[] _body;
        private int _bodySize;

        public ReadOnlySpan<byte> Body => new ReadOnlySpan<byte>(_body, 0, _bodySize);

        internal DeliverArgs(long deliveryTag, ref ContentHeader header, byte[] body)
        {
            Properties = header.Properties;
            DeliveryTag = deliveryTag;
            _body = body;
            _bodySize = (int)header.BodySize;
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
    public class RabbitMQConsumer : IConsumable
    {

        public event EventHandler<DeliverArgs> Received;
        private readonly PipeScheduler _scheduler;
        private byte[] _activeDeliverBody;
        private int _deliverPosition;
        private ContentHeader _activeContent;
        private long _activeDeliveryTag;
        public ConsumeConf Consume;
        public RabbitMQChannel Channel;
        public RabbitMQConsumer(RabbitMQChannel channel, ConsumeConf conf, PipeScheduler scheduler)
        {
            Consume = conf;
            _scheduler = scheduler;
            Channel = channel;
        }
        public RabbitMQConsumer(RabbitMQChannel channel, ConsumeConf conf)
        {
            Consume = conf;
            _scheduler = PipeScheduler.Inline;
            Channel = channel;
        }

        public ValueTask OnDeliveryAsync(ref Deliver deliver)
        {
            _activeDeliveryTag = deliver.DeliverTag;
            return default;
        }
        public ValueTask OnContentAsync(ref ContentHeader header)
        {
            _activeContent = header;
            _activeDeliverBody = ArrayPool<byte>.Shared.Rent((int)header.BodySize);
            return default;
        }

        public ValueTask OnBodyAsync(ReadOnlySequence<byte> body)
        {
            var span = new Span<byte>(_activeDeliverBody, _deliverPosition, (int)body.Length);
            body.CopyTo(span);
            _deliverPosition += (int)body.Length;
            if (_deliverPosition == _activeContent.BodySize)
            {

                var arg = new DeliverArgs(_activeDeliveryTag, ref _activeContent, _activeDeliverBody);
                _scheduler.Schedule(Invoke, arg);
                _activeContent = default;
                _activeDeliveryTag = -1;
            }
            return default;
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
    }
}
